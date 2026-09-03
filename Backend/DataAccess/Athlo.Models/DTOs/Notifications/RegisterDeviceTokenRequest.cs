namespace Athlo.Models.DTOs.Notifications;

public class RegisterDeviceTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = "unknown";
}
