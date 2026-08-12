using System.Text.Json;
using Athlo.Shared.Helpers;
using Athlo.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Athlo.Shared.Filters;

public class ValidationFilter : IActionFilter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        var details = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => new ApiErrorDetail
            {
                Field = ToCamelCase(x.Key),
                Message = string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage
            }))
            .ToList();

        var response = ApiErrorFactory.Create(
            ApiErrorCodes.ValidationFailed,
            "One or more validation errors occurred.",
            details,
            context.HttpContext.TraceIdentifier);

        context.Result = new JsonResult(response, JsonOptions) { StatusCode = StatusCodes.Status400BadRequest };
    }

    public void OnActionExecuted(ActionExecutedContext context) { }

    private static string ToCamelCase(string field) =>
        string.IsNullOrEmpty(field) ? field : char.ToLowerInvariant(field[0]) + field[1..];
}
