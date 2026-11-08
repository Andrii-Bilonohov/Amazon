using System.ComponentModel.DataAnnotations;

namespace Application.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    [Range(1, 90)]
    public int RefreshTokenLifetimeDays { get; set; } = 7;

    // It must match with Notification:EmailConfirmationCodeTtlMinutes
    [Range(1, 1440)]
    public int ConfirmCodeLifetimeMinutes { get; set; } = 15;

    // It must match with Notification:PasswordResetCodeTtlMinutes
    [Range(1, 60)]
    public int PasswordResetCodeLifetimeMinutes { get; set; } = 5;

    [Required] public string PasswordResetSecret { get; set; } = "";
    [Required] public string ConfirmEmailSecret { get; set; } = "";
}