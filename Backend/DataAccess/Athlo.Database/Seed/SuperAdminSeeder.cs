using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Athlo.Shared.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Athlo.Database.Seed;

public static class SuperAdminSeeder
{
    public static readonly Guid SuperAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static async Task EnsureAsync(
        AthloDbContext context,
        IOptions<SuperAdminSettings> settings,
        ILogger logger,
        CancellationToken ct = default)
    {
        var config = settings.Value;
        var email = config.Email.Trim().ToLowerInvariant();
        var password = config.Password;
        var fullName = string.IsNullOrWhiteSpace(config.FullName) ? "Super Admin" : config.FullName.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("SuperAdmin:Email and SuperAdmin:Password must be set in .env.");

        if (password.Length < 12)
            throw new InvalidOperationException("SuperAdmin:Password must be at least 12 characters.");

        var existing = await context.Users
            .FirstOrDefaultAsync(u => u.Id == SuperAdminId || u.Email == email, ct);

        if (existing is null)
        {
            context.Users.Add(new User
            {
                Id = SuperAdminId,
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                InitialWeight = 70,
                CurrentWeight = 70,
                GoalWeight = 70,
                FitnessGoal = FitnessGoal.StayActive,
                Role = UserRole.SuperAdmin
            });

            logger.LogInformation("Super admin account created.");
        }
        else if (existing.Role != UserRole.SuperAdmin)
        {
            throw new InvalidOperationException(
                $"SuperAdmin email '{email}' is already registered as a regular user. Use a dedicated super admin email.");
        }
        else
        {
            var changed = false;

            if (existing.FullName != fullName)
            {
                existing.FullName = fullName;
                changed = true;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, existing.PasswordHash))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                changed = true;
            }

            if (changed)
                logger.LogInformation("Super admin account synchronized from environment.");
        }

        await context.SaveChangesAsync(ct);
    }
}
