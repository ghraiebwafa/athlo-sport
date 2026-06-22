using Athlo.Shared.Helpers;
using Athlo.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Athlo.Shared.Extensions;

public static class FluentValidationExtensions
{
    public static IServiceCollection AddAthloFluentValidation<TAssembly>(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<TAssembly>();
        return services;
    }

    public static async Task<IActionResult?> ToValidationErrorAsync<T>(
        this IValidator<T> validator,
        T model,
        CancellationToken ct = default)
    {
        var result = await validator.ValidateAsync(model, ct);
        if (result.IsValid)
            return null;

        var details = result.Errors.Select(e => new ApiErrorDetail
        {
            Field = string.IsNullOrEmpty(e.PropertyName)
                ? string.Empty
                : char.ToLowerInvariant(e.PropertyName[0]) + e.PropertyName[1..],
            Message = e.ErrorMessage
        });

        var response = ApiErrorFactory.Create(
            ApiErrorCodes.ValidationFailed,
            "One or more validation errors occurred.",
            details);

        return new JsonResult(response) { StatusCode = StatusCodes.Status400BadRequest };
    }
}
