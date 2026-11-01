using System.Security.Cryptography;
using System.Text;
using Application.Abstractions;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Mappers;
using Domain.Abstractions;
using EventBus.UsersEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedKernel.Responses;

namespace Application.Services;

public class AuthentificationService : IAuthentificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthentificationService> _logger;
    private readonly byte[] _resetSecret;

    public AuthentificationService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IEventPublisher eventPublisher,
        IConfiguration configuration,
        ILogger<AuthentificationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var secretBase64 = _configuration["PASSWORDRESETCODE:SECRET"]
                           ?? throw new InvalidOperationException("PASSWORDRESETCODE:SECRET is not configured.");
        _resetSecret = Convert.FromBase64String(secretBase64);
    }

    public async Task<ItemResponse<AuthUserResponse>> RegisterUser(
        RegisterUserRequest userRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = NormalizeEmail(userRequest.Email);
            ValidatePassword(userRequest.Password);

            var existingUser = await _unitOfWork.Users.GetByAsync(u => u.Email == email, cancellationToken);
            if (existingUser != null)
                return ItemResponse<AuthUserResponse>.Failure("Email already registered");

            var secureRequest = userRequest with
            {
                Email = email,
                Password = PasswordHasher(userRequest.Password)
            };
            var user = AuthentificationMapper.FromAuthUserRequestToUserRegisterEntity(secureRequest);

            _unitOfWork.Users.Add(user);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

            await _eventPublisher.PublishAsync(
                new UserRegistered(user.Id, user.Email, user.Role.ToString(), DateTime.UtcNow),
                cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return ItemResponse<AuthUserResponse>.Success(new AuthUserResponse(accessToken, refreshToken));
        }
        catch (ArgumentException ex)
        {
            return ItemResponse<AuthUserResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed");
            return ItemResponse<AuthUserResponse>.Failure("An error occurred. Please try again later.");
        }
    }

    public async Task<ItemResponse<AuthUserResponse>> LoginUser(
        LoginUserRequest userRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = NormalizeEmail(userRequest.Email);
            var user = await _unitOfWork.Users.GetByAsync(u => u.Email == email, cancellationToken);

            if (user == null || !VerifyPassword(userRequest.Password, user.PasswordHash))
                return ItemResponse<AuthUserResponse>.Failure("Invalid email or password.");

            if (user.IsBlocked)
                return ItemResponse<AuthUserResponse>.Failure("Your account has been blocked.");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);

            return ItemResponse<AuthUserResponse>.Success(new AuthUserResponse(accessToken, refreshToken));
        }
        catch (ArgumentException ex)
        {
            return ItemResponse<AuthUserResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return ItemResponse<AuthUserResponse>.Failure("An error occurred. Please try again later.");
        }
    }

    public async Task<InfoResponse> InitiatePasswordReset(
        ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = NormalizeEmail(request.Email);
            var user = await _unitOfWork.Users.GetByAsync(u => u.Email == email, cancellationToken);

            if (user == null || user.IsBlocked)
                return InfoResponse.Success("If the account exists, a verification code has been sent.");

            var code = GenerateCode();

            user.SetPasswordResetCode(
                HashCode(code, user.Id, _resetSecret),
                DateTime.UtcNow.AddMinutes(5));

            await _eventPublisher.PublishAsync(
                new UserRequestedPasswordResetEvent(user.Id, user.Email, code, DateTime.UtcNow),
                cancellationToken);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);

            return InfoResponse.Success("If the account exists, a verification code has been sent.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset initiation failed");
            return InfoResponse.Failure("An error occurred. Please try again later.");
        }
    }

    public async Task<InfoResponse> CompletePasswordReset(
        ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidatePassword(request.NewPassword);

            var email = NormalizeEmail(request.Email);
            var user = await _unitOfWork.Users.GetByAsync(u => u.Email == email, cancellationToken);

            if (user == null || user.IsBlocked || !user.CanUseResetCode(DateTime.UtcNow))
                return InfoResponse.Failure("Invalid or expired code.");

            var expected = Encoding.UTF8.GetBytes(user.PasswordResetCode!);
            var actual = Encoding.UTF8.GetBytes(HashCode(request.Code, user.Id, _resetSecret));

            if (!CryptographicOperations.FixedTimeEquals(expected, actual))
            {
                user.RegisterFailedResetAttempt();
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync(cancellationToken);
                return InfoResponse.Failure("Invalid or expired code.");
            }

            var newPasswordHash = PasswordHasher(request.NewPassword);
            user.ChangePassword(newPasswordHash);
            user.ClearPasswordResetCode();
            user.ClearRefreshToken();

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);

            return InfoResponse.Success("Password has been changed successfully. Please log in again.");
        }
        catch (ArgumentException ex)
        {
            return InfoResponse.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset completion failed");
            return InfoResponse.Failure("An error occurred. Please try again later.");
        }
    }

    // ---------- helpers ----------

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");
        return email.Trim().ToLowerInvariant();
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters.");
    }

    private static string PasswordHasher(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    private static bool VerifyPassword(string password, string hashedPassword)
        => BCrypt.Net.BCrypt.Verify(password, hashedPassword);

    private static string GenerateCode()
        => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string HashCode(string code, Guid userId, byte[] secret)
    {
        var data = Encoding.UTF8.GetBytes($"{userId}:{code}");
        return Convert.ToHexString(HMACSHA256.HashData(secret, data));
    }
}
