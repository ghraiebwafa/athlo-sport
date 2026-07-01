using Microsoft.Extensions.Logging;

namespace Athlo.Shared.Email;

/// <summary>Development/testing sender — logs reset links instead of sending mail.</summary>
public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Password reset requested for {Email}. Check application logs policy for token delivery in dev.",
            toEmail);

        return Task.CompletedTask;
    }
}
