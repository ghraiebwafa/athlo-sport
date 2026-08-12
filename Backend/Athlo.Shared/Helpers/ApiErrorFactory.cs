using Athlo.Shared.Models;

namespace Athlo.Shared.Helpers;

public static class ApiErrorFactory
{
    public static ApiErrorResponse Create(
        string code,
        string message,
        IEnumerable<ApiErrorDetail>? details = null,
        string? traceId = null) =>
        new()
        {
            Api = new ApiErrorContainer
            {
                Error = new ApiError
                {
                    Code = code,
                    Message = message,
                    TraceId = traceId,
                    Timestamp = DateTimeOffset.UtcNow,
                    Details = details?.ToList() ?? []
                }
            }
        };

    public static ApiErrorResponse FromCode(string code, string message, string? traceId = null) =>
        Create(code, message, traceId: traceId);
}
