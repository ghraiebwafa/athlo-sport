using System.Net;
using System.Net.Mail;
using Athlo.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Athlo.Shared.Email;

public class SmtpEmailSender(IOptions<SmtpSettings> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Host))
            throw new InvalidOperationException("SMTP host is not configured.");

        var from = string.IsNullOrWhiteSpace(settings.From) ? settings.User : settings.From;
        using var message = new MailMessage(from, toEmail)
        {
            Subject = "ATHLO — Reset your password",
            Body = EmailTemplates.PasswordReset(resetToken),
            IsBodyHtml = true
        };
        message.AlternateViews.Add(
            AlternateView.CreateAlternateViewFromString(
                EmailTemplates.PlainPasswordReset(resetToken),
                null,
                "text/plain"));

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(settings.User)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(settings.User, settings.Password)
        };

        ct.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, ct);
        logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }
}
