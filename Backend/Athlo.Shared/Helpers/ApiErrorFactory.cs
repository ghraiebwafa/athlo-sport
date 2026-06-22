using Athlo.Shared.Models;

namespace Athlo.Shared.Helpers;

public static class ApiErrorFactory
{
    public static ApiErrorResponse Create(string code, string message, IEnumerable<ApiErrorDetail>? details = null) =>
        new()
        {
            Api = new ApiErrorContainer
            {
                Error = new ApiError
                {
                    Code = code,
                    Message = message,
                    Details = details?.ToList() ?? []
                }
            }
        };

    public static ApiErrorResponse FromCode(string code, string message) =>
        Create(code, message);
}
