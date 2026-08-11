using System.ComponentModel.DataAnnotations;

namespace Athlo.Models.DTOs.Workouts;

public class LogSetRequest
{
    [Required]
    public Guid ProgramExerciseId { get; set; }

    public int SetNumber { get; set; }

    public int RepsCompleted { get; set; }

    public decimal? WeightKg { get; set; }

    public bool Completed { get; set; } = true;
}
