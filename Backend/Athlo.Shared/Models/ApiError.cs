namespace Athlo.Shared.Models;

/// <summary>
/// Standard API error envelope: { "api": { "error": { ... } } }.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>Root wrapper matching the API response schema.</summary>
    public ApiErrorContainer Api { get; set; } = new();
}

/// <summary>
/// Container for the error payload within an <see cref="ApiErrorResponse"/>.
/// </summary>
public class ApiErrorContainer
{
    /// <summary>The error details returned to the client.</summary>
    public ApiError Error { get; set; } = new();
}

/// <summary>
/// Machine-readable error body included in failed API responses.
/// </summary>
public class ApiError
{
    /// <summary>
    /// Stable error code for client-side handling. Use constants from <see cref="ApiErrorCodes"/>.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Human-readable description of the error.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Request trace identifier for correlating client reports with server logs.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>UTC timestamp when the error was generated.</summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional field-level validation errors. Empty for non-validation failures.
    /// </summary>
    public IReadOnlyList<ApiErrorDetail> Details { get; set; } = [];
}

/// <summary>
/// A single field-level validation failure within an <see cref="ApiError"/>.
/// </summary>
public class ApiErrorDetail
{
    /// <summary>JSON property or form field name that failed validation.</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Validation message for this field.</summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Stable string constants for <see cref="ApiError.Code"/> values used across services.
/// </summary>
public static class ApiErrorCodes
{
    /// <summary>Request body or query parameters failed validation (HTTP 400).</summary>
    public const string ValidationFailed = "VALIDATION_FAILED";

    /// <summary>Missing or invalid authentication credentials (HTTP 401).</summary>
    public const string Unauthorized = "UNAUTHORIZED";

    /// <summary>Authenticated but not permitted to perform the action (HTTP 403).</summary>
    public const string Forbidden = "FORBIDDEN";

    /// <summary>Requested resource does not exist or is intentionally hidden (HTTP 404).</summary>
    public const string NotFound = "NOT_FOUND";

    /// <summary>Request conflicts with current resource state (HTTP 409).</summary>
    public const string Conflict = "CONFLICT";

    /// <summary>Malformed or semantically invalid request (HTTP 400).</summary>
    public const string BadRequest = "BAD_REQUEST";

    /// <summary>Unexpected server failure (HTTP 500).</summary>
    public const string InternalError = "INTERNAL_ERROR";

    /// <summary>Too many requests from the client (HTTP 429).</summary>
    public const string RateLimited = "RATE_LIMITED";
}
