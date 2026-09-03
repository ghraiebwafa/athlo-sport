using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Athlo.ManagementService.Services;

public interface IPushNotificationSender
{
    Task SendAsync(IReadOnlyList<string> tokens, string title, string body, CancellationToken ct = default);
}

/// <summary>Development fallback — logs instead of sending push.</summary>
public class LoggingPushNotificationSender(ILogger<LoggingPushNotificationSender> logger) : IPushNotificationSender
{
    public Task SendAsync(IReadOnlyList<string> tokens, string title, string body, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Push notification Title={Title} Body={Body} Recipients={Count}",
            title, body, tokens.Count);
        return Task.CompletedTask;
    }
}

/// <summary>Sends via Expo Push API (https://docs.expo.dev/push-notifications/sending-notifications/).</summary>
public class ExpoPushNotificationSender(
    HttpClient httpClient,
    ILogger<ExpoPushNotificationSender> logger) : IPushNotificationSender
{
    public async Task SendAsync(IReadOnlyList<string> tokens, string title, string body, CancellationToken ct = default)
    {
        if (tokens.Count == 0)
            return;

        var messages = tokens.Select(token => new ExpoPushMessage
        {
            To = token,
            Title = title,
            Body = body,
            Sound = "default"
        }).ToList();

        using var response = await httpClient.PostAsJsonAsync(
            "https://exp.host/--/api/v2/push/send",
            messages,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Expo push failed Status={Status} Body={Body}", response.StatusCode, error);
            return;
        }

        logger.LogInformation("Expo push sent Title={Title} Recipients={Count}", title, tokens.Count);
    }

    private sealed class ExpoPushMessage
    {
        [JsonPropertyName("to")]
        public string To { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("sound")]
        public string Sound { get; set; } = "default";
    }
}
