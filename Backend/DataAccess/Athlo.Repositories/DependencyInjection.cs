using Athlo.Repositories.Categories;
using Athlo.Repositories.Exercises;
using Athlo.Repositories.PasswordResetTokens;
using Athlo.Repositories.Programs;
using Athlo.Repositories.RefreshTokens;
using Athlo.Repositories.Users;
using Athlo.Repositories.Workouts;
using Microsoft.Extensions.DependencyInjection;

namespace Athlo.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddAthloRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IProgramRepository, ProgramRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
        return services;
    }
}
