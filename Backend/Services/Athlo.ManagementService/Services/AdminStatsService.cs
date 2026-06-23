using Athlo.Models.DTOs.Admin;
using Athlo.Repositories.Exercises;
using Athlo.Repositories.Programs;
using Athlo.Repositories.Users;
using Athlo.Repositories.Workouts;
using Athlo.Shared.Enums;

namespace Athlo.ManagementService.Services;

public interface IAdminStatsService
{
    Task<AdminDashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);
}

public class AdminStatsService(
    IUserRepository userRepository,
    IProgramRepository programRepository,
    IExerciseRepository exerciseRepository,
    IWorkoutSessionRepository workoutSessionRepository) : IAdminStatsService
{
    public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var adminCount = await userRepository.CountAsync(UserRole.Admin, ct)
            + await userRepository.CountAsync(UserRole.SuperAdmin, ct);

        return new AdminDashboardStatsDto
        {
            TotalUsers = await userRepository.CountAsync(ct: ct),
            TotalAdmins = adminCount,
            TotalPrograms = await programRepository.CountAsync(ct),
            TotalExercises = await exerciseRepository.CountAsync(ct),
            CompletedWorkoutsToday = await workoutSessionRepository.CountCompletedTodayAsync(ct),
            ActiveWorkoutsNow = await workoutSessionRepository.CountActiveAsync(ct)
        };
    }
}
