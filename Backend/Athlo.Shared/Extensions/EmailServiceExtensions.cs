using Athlo.Shared.Email;
using Athlo.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Athlo.Shared.Extensions;

public static class EmailServiceExtensions
{
    public static IServiceCollection AddAthloEmail(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        var smtpHost = configuration[$"{SmtpSettings.SectionName}:Host"];

        if (!string.IsNullOrWhiteSpace(smtpHost))
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        else if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
        else
            throw new InvalidOperationException(
                "Smtp:Host must be configured in production so password reset emails can be sent.");

        return services;
    }
}
