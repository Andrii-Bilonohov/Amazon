using Domain.Enums;
using Domain.Events;
using Domain.Validators;
using SharedKernel;

namespace Domain.Entities;


public class User : AggregateRoot
{
    private const int MaxConfirmationAttempts = 5;
    private const int MaxResetAttempts = 5;

    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string Email { get; private set; } = null!;
    public bool IsEmailConfirmed { get; private set; }
    
    public string? EmailConfirmationCodeHash { get; private set; }
    public DateTime? EmailConfirmationExpiresAt { get; private set; }
    public int EmailConfirmationAttempts { get; private set; }

    public string? AvatarUrl { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? PhoneNumber { get; private set; }
    public Role Role { get; private set; }
    public bool IsBlocked { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }
    
    public string? PasswordResetCode { get; private set; }
    public DateTime? PasswordResetExpiresAt { get; private set; }
    public int PasswordResetAttempts { get; private set; }

    private User() { } 

    private User(string email, string passwordHash, Role role)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public static User Register(string email, string passwordHash, Role role)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required.");
        email = email.Trim().ToLowerInvariant();
        if (!IdentityValidators.IsValidEmail(email)) throw new ArgumentException("Invalid email address.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password required.");
        if (role != Role.Seller && role != Role.Buyer) throw new ArgumentException("Invalid role.");

        return new User(email, passwordHash, role);
    }

    public void UpdateProfile(string? firstName, string? lastName, string? phoneNumber, DateOnly? birthDate, string? avatarUrl)
    {
        if (birthDate != null && birthDate > DateOnly.FromDateTime(DateTime.UtcNow)) throw new ArgumentException("Invalid birth date.");
        if (phoneNumber != null && !IdentityValidators.IsValidPhone(phoneNumber)) throw new ArgumentException("Invalid phone number.");
        if (avatarUrl != null && !IdentityValidators.IsValidUrl(avatarUrl)) throw new ArgumentException("Invalid avatar url.");

        FirstName = firstName ?? FirstName;
        LastName = lastName ?? LastName;
        PhoneNumber = phoneNumber ?? PhoneNumber;
        AvatarUrl = avatarUrl ?? AvatarUrl;
        BirthDate = birthDate ?? BirthDate;

        Touch();
    }

    public void ChangeRole(Role newRole)
    {
        if (newRole != Role.Seller && newRole != Role.Buyer) throw new ArgumentException("Invalid role.");
        if (Role == newRole) return;

        Role = newRole;
        ClearRefreshToken(); 
        Raise(new UserRoleChanged(Id, newRole));
        Touch();
    }

    public void UpdateBlockStatus(bool isBlocked)
    {
        if (IsBlocked == isBlocked) return;

        IsBlocked = isBlocked;
        RefreshToken = null;
        RefreshTokenExpiryTime = null;

        Raise(new UserBlockStatusChanged(Id, isBlocked));
        Touch();
    }

    public void MarkDeleted()
    {
        if (IsDeleted) return;

        Delete();
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        Raise(new UserDeleted(Id));
    }
    

    public void UpdateRefreshToken(string refreshTokenHash, DateTime expiryTime)
    {
        RefreshToken = refreshTokenHash;
        RefreshTokenExpiryTime = expiryTime;
        Touch();
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        Touch();
    }
    
    public void RequestPasswordReset(string codeHash, string plainCode, DateTime expiresAt)
    {
        PasswordResetCode = codeHash;
        PasswordResetExpiresAt = expiresAt;
        PasswordResetAttempts = 0;

        Raise(new PasswordResetRequested(Id, Email, plainCode));
        Touch();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("Password required.");

        PasswordHash = newPasswordHash;
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        PasswordResetCode = null;
        PasswordResetExpiresAt = null;
        PasswordResetAttempts = 0;
        Touch();
    }
    

    public void SetPasswordResetCode(string codeHash, DateTime expiresAt)
    {
        PasswordResetCode = codeHash;
        PasswordResetExpiresAt = expiresAt;
        PasswordResetAttempts = 0;
        Touch();
    }
    
    public ResetCodeResult VerifyPasswordResetCode(string codeHash, DateTime now)
    {
        if (PasswordResetCode is null || PasswordResetExpiresAt is null || PasswordResetExpiresAt <= now)
            return ResetCodeResult.Expired;
        if (PasswordResetAttempts >= MaxResetAttempts)
            return ResetCodeResult.TooManyAttempts;

        if (!HashesEqual(PasswordResetCode, codeHash))
        {
            PasswordResetAttempts++;
            Touch();
            return ResetCodeResult.Invalid;
        }

        return ResetCodeResult.Valid; 
    }
    
    public void RequestEmailConfirmation(string codeHash, string plainCode, DateTime now, TimeSpan ttl)
    {
        if (IsEmailConfirmed) throw new InvalidOperationException("Email already confirmed.");

        EmailConfirmationCodeHash = codeHash;
        EmailConfirmationExpiresAt = now.Add(ttl);
        EmailConfirmationAttempts = 0;

        Raise(new EmailConfirmationRequested(Id, Email, plainCode));
        Touch();
    }
    
    public ConfirmEmailResult ConfirmEmail(string codeHash, DateTime now)
    {
        if (IsEmailConfirmed) return ConfirmEmailResult.AlreadyConfirmed;

        if (EmailConfirmationCodeHash is null || EmailConfirmationExpiresAt is null || EmailConfirmationExpiresAt <= now)
            return ConfirmEmailResult.Expired;
        if (EmailConfirmationAttempts >= MaxConfirmationAttempts)
            return ConfirmEmailResult.TooManyAttempts;

        if (!HashesEqual(EmailConfirmationCodeHash, codeHash))
        {
            EmailConfirmationAttempts++;
            Touch();
            return ConfirmEmailResult.Invalid;
        }

        IsEmailConfirmed = true;
        EmailConfirmationCodeHash = null;
        EmailConfirmationExpiresAt = null;
        EmailConfirmationAttempts = 0;

        Raise(new UserEmailConfirmed(Id, Email, Role));
        Touch();
        return ConfirmEmailResult.Confirmed;
    }

    private static bool HashesEqual(string a, string b) =>
        System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(a),
            System.Text.Encoding.UTF8.GetBytes(b));
}