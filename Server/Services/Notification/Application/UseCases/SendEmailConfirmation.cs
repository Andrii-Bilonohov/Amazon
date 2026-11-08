using System.Net;
using Application.Abstractions;
using Application.Emails;
using Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.UseCases;

public sealed class SendEmailConfirmation
{
    
    private readonly IEmailSender _emailSender;
    private readonly TimeProvider _clock;
    private readonly IOptions<AuthOptions> _options;
    private readonly ILogger<SendEmailConfirmation> _logger;


    public SendEmailConfirmation(IEmailSender emailSender, TimeProvider clock, IOptions<AuthOptions> options, ILogger<SendEmailConfirmation> logger)
    {
        _emailSender = emailSender ??  throw new ArgumentNullException(nameof(emailSender));
        _clock = clock ??  throw new ArgumentNullException(nameof(clock));
        _options = options ??  throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task ExecuteAsync(Guid eventId, Guid userId, string email, string code,
        DateTime occurredOn, CancellationToken ct)
    {
        var o = _options.Value;
        var maxAge = TimeSpan.FromMinutes(o.ConfirmCodeLifetimeMinutes);
        
        if (_clock.GetUtcNow().UtcDateTime - occurredOn > maxAge)
        {
            _logger.LogWarning("Skipped stale email confirmation. EventId={EventId} UserId={UserId}", eventId, userId);
            return;
        }

        var body = EmailTemplates.EmailConfirmation(code, o.ConfirmCodeLifetimeMinutes);

        await _emailSender.SendAsync(new EmailMessage(email, "Підтвердження пошти", body), ct);
        _logger.LogInformation("Email confirmation sent. EventId={EventId} UserId={UserId}", eventId, userId);
    }
}
