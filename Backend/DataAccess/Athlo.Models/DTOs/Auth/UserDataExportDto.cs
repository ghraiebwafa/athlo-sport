namespace Athlo.Models.DTOs.Auth;

/// <summary>GDPR-style export of the authenticated user's Athlo data.</summary>
public class UserDataExportDto
{
    public DateTimeOffset ExportedAt { get; set; } = DateTimeOffset.UtcNow;
    public UserProfileResponse Profile { get; set; } = null!;
    public UserPreferencesDto Preferences { get; set; } = null!;
    public IReadOnlyList<ExportedWorkoutDto> Workouts { get; set; } = [];
    public IReadOnlyList<ExportedSavedProgramDto> SavedPrograms { get; set; } = [];
}

public class ExportedWorkoutDto
{
    public Guid SessionId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CaloriesBurned { get; set; }
    public int? DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public IReadOnlyList<ExportedSetDto> Sets { get; set; } = [];
}

public class ExportedSetDto
{
    public string ExerciseName { get; set; } = string.Empty;
    public int SetNumber { get; set; }
    public int RepsCompleted { get; set; }
    public decimal? WeightKg { get; set; }
    public bool Completed { get; set; }
    public DateTime LoggedAt { get; set; }
}

public class ExportedSavedProgramDto
{
    public Guid ProgramId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime SavedAt { get; set; }
}
