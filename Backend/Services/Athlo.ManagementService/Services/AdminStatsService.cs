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
        var usersTask = userRepository.CountAsync(ct: ct);
        var adminsTask = userRepository.CountAsync(UserRole.Admin, ct);
        var superAdminsTask = userRepository.CountAsync(UserRole.SuperAdmin, ct);
        var programsTask = programRepository.CountAsync(ct);
        var exercisesTask = exerciseRepository.CountAsync(ct);
        var completedTodayTask = workoutSessionRepository.CountCompletedTodayAsync(ct);
        var activeNowTask = workoutSessionRepository.CountActiveAsync(ct);

        await Task.WhenAll(usersTask, adminsTask, superAdminsTask, programsTask, exercisesTask, completedTodayTask, activeNowTask);

        return new AdminDashboardStatsDto
        {
            TotalUsers = usersTask.Result,
            TotalAdmins = adminsTask.Result + superAdminsTask.Result,
            TotalPrograms = programsTask.Result,
            TotalExercises = exercisesTask.Result,
            CompletedWorkoutsToday = completedTodayTask.Result,
            ActiveWorkoutsNow = activeNowTask.Result
        };
    }
}
