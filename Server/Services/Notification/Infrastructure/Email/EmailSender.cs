using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Application.Abstractions;
using Application.Options;

namespace Infrastructure.Email;

public sealed class EmailSender : IEmailSender
{
    private readonly IOptions<EmailOptions> _options; 
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailOptions> options, ILogger<EmailSender> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        var o = _options.Value;

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(o.FromName, o.User));

        try
        {
            mime.To.Add(MailboxAddress.Parse(message.To));
        }
        catch (ParseException ex)
        {
            throw new InvalidRecipientException("Invalid recipient address.", ex);
        }

        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(o.Host, o.Port, SecureSocketOptions.Auto, ct);
        await client.AuthenticateAsync(o.User, o.Password, ct);
        await client.SendAsync(mime, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Email sent via {Host}:{Port} from {User}", o.Host, o.Port, o.User);
    }
}