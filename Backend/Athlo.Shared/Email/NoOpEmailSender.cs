namespace Athlo.Shared.Email;

/// <summary>Production placeholder — completes without sending mail. Replace with SMTP/SendGrid when ready.</summary>
public class NoOpEmailSender : IEmailSender
{
    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default) =>
        Task.CompletedTask;
}
