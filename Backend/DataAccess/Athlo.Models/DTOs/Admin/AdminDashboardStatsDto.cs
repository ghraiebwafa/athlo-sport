namespace Athlo.Models.DTOs.Admin;

public class AdminDashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalPrograms { get; set; }
    public int TotalExercises { get; set; }
    public int CompletedWorkoutsToday { get; set; }
    public int ActiveWorkoutsNow { get; set; }
}
