namespace Athlo.Models.DTOs.Auth;

public class ForgotPasswordResponse
{
    public string Message { get; set; } = "If an account exists, a reset link has been sent.";
    public string? ResetToken { get; set; }
}
