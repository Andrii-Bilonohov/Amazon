using System.Net;
using Application.Abstractions;
using Application.Emails;
using Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.UseCases;

public sealed class SendPasswordReset(
    IEmailSender sender,
    TimeProvider clock,
    IOptions<AuthOptions> options,
    ILogger<SendPasswordReset> logger)
{
    public async Task ExecuteAsync(Guid eventId, Guid userId, string email, string code,
        DateTime occurredOn, CancellationToken ct)
    {
        var o = options.Value;
        var maxAge = TimeSpan.FromMinutes(o.PasswordResetCodeLifetimeMinutes);

        if (clock.GetUtcNow().UtcDateTime - occurredOn > maxAge)
        {
            logger.LogWarning("Skipped stale password reset. EventId={EventId} UserId={UserId}", eventId, userId);
            return;
        }

        var body = EmailTemplates.PasswordReset(code, o.ConfirmCodeLifetimeMinutes);

        await sender.SendAsync(new EmailMessage(email, "Скидання пароля", body), ct);
        logger.LogInformation("Password reset email sent. EventId={EventId} UserId={UserId}", eventId, userId);
    }
}
