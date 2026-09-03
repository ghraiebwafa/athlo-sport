namespace Athlo.Models.DTOs.Auth;

/// <summary>Request body for permanent account deletion.</summary>
public class DeleteAccountRequest
{
    /// <summary>Current password confirmation.</summary>
    public string Password { get; set; } = string.Empty;
}
