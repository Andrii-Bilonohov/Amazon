using Domain.Enums;
using Domain.Validators;
using SharedKernel;

namespace Domain.Entities;

public class User : BaseEntity
{
    public string? FirstName { get; private set; } 
    public string? LastName { get; private set; }
    public string Email { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? PhoneNumber { get; private set; }
    public Role Role { get; private set; }
    public bool IsBlocked { get; private set; } = false;
    public string PasswordHash { get; private set; }
    
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }
    
    public string? PasswordResetCode { get; private set; }
    public DateTime? PasswordResetExpiresAt { get; private set; }
    public int PasswordResetAttempts { get; private set; }

    private User(string email, string passwordHash, Role role)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }
    
    public static User Register(string email, string passwordHash, Role role)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required.");
        if(!IdentityValidators.IsValidEmail(email)) throw new ArgumentException("Invalid email address.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password required.");
        if (role != Role.Seller && role != Role.Buyer) throw new ArgumentException("Invalid role.");
        
        return new(email, passwordHash, role);
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
    
    public void UpdateBlockStatus(bool isBlocked)
    {
        IsBlocked = isBlocked;
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        
        Touch();
    }
    
    public void UpdateRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
        
        Touch();
    }
    
    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        Touch();
    }

    public void ChangePassword(string newPassword)
    {
        PasswordHash = newPassword;
        
        Touch();
    }
    
    public void SetPasswordResetCode(string code, DateTime expiresAt)
    {
        PasswordResetCode = code;
        PasswordResetExpiresAt = expiresAt;
        PasswordResetAttempts = 0;
    }
    
    public void RegisterFailedResetAttempt() => PasswordResetAttempts++;

    public void ClearPasswordResetCode()
    {
        PasswordResetCode = null;
        PasswordResetExpiresAt = null;
        PasswordResetAttempts = 0;
    }

    public bool CanUseResetCode(DateTime now) =>
        PasswordResetCode != null
        && PasswordResetExpiresAt > now
        && PasswordResetAttempts < 5;
}
