using System.Text.Json;
using Athlo.Shared.Exceptions;
using Athlo.Shared.Helpers;
using Athlo.Shared.Models;

namespace Athlo.Shared.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            var code = MapCode(ex.StatusCode);
            logger.LogWarning(
                ex,
                "Application error {ErrorCode} ({StatusCode}) TraceId={TraceId}: {Message}",
                code,
                ex.StatusCode,
                context.TraceIdentifier,
                ex.Message);
            await WriteErrorAsync(context, ex.StatusCode, code, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(
                ex,
                "Unauthorized access TraceId={TraceId}",
                context.TraceIdentifier);
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, ApiErrorCodes.Unauthorized, "Unauthorized.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception TraceId={TraceId}",
                context.TraceIdentifier);
            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                ApiErrorCodes.InternalError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = ApiErrorFactory.Create(code, message, traceId: context.TraceIdentifier);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static string MapCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => ApiErrorCodes.BadRequest,
        StatusCodes.Status401Unauthorized => ApiErrorCodes.Unauthorized,
        StatusCodes.Status403Forbidden => ApiErrorCodes.Forbidden,
        StatusCodes.Status404NotFound => ApiErrorCodes.NotFound,
        StatusCodes.Status409Conflict => ApiErrorCodes.Conflict,
        StatusCodes.Status429TooManyRequests => ApiErrorCodes.RateLimited,
        _ => ApiErrorCodes.InternalError
    };
}
