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
        else if (environment.IsDevelopment())
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
        else
            services.AddSingleton<IEmailSender, NoOpEmailSender>();

        return services;
    }
}
