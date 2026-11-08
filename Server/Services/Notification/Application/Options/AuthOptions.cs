using System.ComponentModel.DataAnnotations;

namespace Application.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    // It must match with Authentification:EmailConfirmationCodeTtlMinutes
    [Range(1, 1440)]
    public int ConfirmCodeLifetimeMinutes { get; set; } = 15;

    // It must match with Authentification:PasswordResetCodeTtlMinutes
    [Range(1, 60)]
    public int PasswordResetCodeLifetimeMinutes { get; set; } = 5;
}