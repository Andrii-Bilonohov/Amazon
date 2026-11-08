using System.Security.Cryptography;
using System.Text;
using Application.Abstractions;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Mappers;
using Application.Options;
using Domain.Abstractions;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using SharedKernel.Responses;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class AuthentificationService : IAuthentificationService
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly TimeProvider _clock;
    private readonly ILogger<AuthentificationService> _logger;
    
    private readonly byte[] _passwordResetSecret;
    private readonly byte[] _confirmEmailSecret;
    private readonly TimeSpan _refreshTokenLifetime;
    private readonly TimeSpan _confirmCodeLifetime;
    private readonly TimeSpan _resetCodeLifetime;
    
    public AuthentificationService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        TimeProvider clock,
        IOptions<AuthOptions> options,
        ILogger<AuthentificationService> logger
        )
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var o = options.Value;
        _passwordResetSecret = Convert.FromBase64String(o.PasswordResetSecret);
        _confirmEmailSecret = Convert.FromBase64String(o.ConfirmEmailSecret);
        _refreshTokenLifetime = TimeSpan.FromDays(o.RefreshTokenLifetimeDays);
        _confirmCodeLifetime = TimeSpan.FromMinutes(o.ConfirmCodeLifetimeMinutes);
        _resetCodeLifetime = TimeSpan.FromMinutes(o.PasswordResetCodeLifetimeMinutes);
    }

    private DateTime Now => _clock.GetUtcNow().UtcDateTime;
    
    
    public async Task<InfoResponse> InitiateRegisterUserAsync(
        RegisterUserRequest userRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = NormalizeEmail(userRequest.Email);
            ValidatePassword(userRequest.Password);

            var existing = await _unitOfWork.Users.GetByAsync(u => u.Email == email, cancellationToken);

            if (existing is { IsEmailConfirmed: true })
                return InfoResponse.Failure("Email already registered");

            var passwordHash = PasswordHasher(userRequest.Password);

            var user = existing;
            if (user is null)
            {
                var secureRequest = userRequest with { Email = email, Password = passwordHash };
                user = AuthentificationMapper.FromAuthUserRequestToUserRegisterEntity(secureRequest);
                _unitOfWork.Users.Add(user);
            }
            user.ChangePassword(passwordHash);

            var code = GenerateCode();
            user.RequestEmailConfirmation(
                HashCode(code, user.Id, _confirmEmailSecret), code, Now, _confirmCodeLifetime);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);

            return InfoResponse.Success("Verification code has been sent to your email.");
        }
        catch (ArgumentException ex)
        {
            return InfoResponse.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed");
            return InfoResponse.Failure("An error occurred. Please try again later.");
        }
    }
    
    public async Task<ItemResponse<AuthUserResponse>> ConfirmEmailAsync(
        ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = NormalizeEmail(request.Email);
            var user = await _unitOfWork.Users.GetByAsync(u => u.Email == email, cancellationToken);

            if (user == null || user.IsBlocked)
                return ItemResponse<AuthUserResponse>.Failure("Invalid or expired code.");

            var result = user.ConfirmEmail(HashCode(request.Code, user.Id, _confirmEmailSecret), Now);
            
            if (result != ConfirmEmailResult.Confirmed)
            {
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync(cancellationToken);
                return ItemResponse<AuthUserResponse>.Failure("Invalid or expired code.");
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.UpdateRefreshToken(HashToken(refreshToken), Now.Add(_refreshTokenLifetime));
            
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);
            return ItemResponse<AuthUserResponse>.Success(new AuthUserResponse(accessToken, refreshToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email confirmation failed");
            return ItemResponse<AuthUserResponse>.Failure("An error occurred. Please try again later.");
        }
    }

    public async Task<ItemResponse<AuthUserResponse>> LoginUserAsync(
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
            user.UpdateRefreshToken(HashToken(refreshToken), Now.Add(_refreshTokenLifetime));

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

    public async Task<InfoResponse> InitiatePasswordResetAsync(
        ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = NormalizeEmail(request.Email);
            var user = await _unitOfWork.Users.GetByAsync(u => u.Email == email, cancellationToken);

            if (user == null || user.IsBlocked)
                return InfoResponse.Success("If the account exists, a verification code has been sent.");

            var code = GenerateCode();
            
            user.RequestPasswordReset(HashCode(code, user.Id, _passwordResetSecret), code, Now.Add(_resetCodeLifetime));

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);

            return InfoResponse.Success("If the account exists, a verification code has been sent.");
        }
        catch (ArgumentException ex)
        {
            return InfoResponse.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset initiation failed");
            return InfoResponse.Failure("An error occurred. Please try again later.");
        }
    }

    public async Task<InfoResponse> ConfirmPasswordResetAsync(
        ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidatePassword(request.NewPassword);

            var email = NormalizeEmail(request.Email);
            var user = await _unitOfWork.Users.GetByAsync(u => u.Email == email, cancellationToken);

            if (user == null || user.IsBlocked)
                return InfoResponse.Failure("Invalid or expired code.");

            var result = user.VerifyPasswordResetCode(HashCode(request.Code, user.Id, _passwordResetSecret), Now);

            if (result != ResetCodeResult.Valid)
            {
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync(cancellationToken);
                return InfoResponse.Failure("Invalid or expired code.");
            }
            
            user.ChangePassword(PasswordHasher(request.NewPassword));
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

    private static string PasswordHasher(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    private static bool VerifyPassword(string password, string hashedPassword)
        => BCrypt.Net.BCrypt.Verify(password, hashedPassword);

    private static string GenerateCode()
        => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    // hash code
    private static string HashCode(string code, Guid userId, byte[] secret)
        => Convert.ToHexString(HMACSHA256.HashData(secret, Encoding.UTF8.GetBytes($"{userId}:{code}")));

    // hash refresh token
    private static string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}