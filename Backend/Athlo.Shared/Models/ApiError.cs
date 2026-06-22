namespace Athlo.Shared.Models;

public class ApiErrorResponse
{
    public ApiErrorContainer Api { get; set; } = new();
}

public class ApiErrorContainer
{
    public ApiError Error { get; set; } = new();
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<ApiErrorDetail> Details { get; set; } = [];
}

public class ApiErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public static class ApiErrorCodes
{
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string BadRequest = "BAD_REQUEST";
    public const string InternalError = "INTERNAL_ERROR";
    public const string RateLimited = "RATE_LIMITED";
}
