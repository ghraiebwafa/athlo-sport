using System.ComponentModel.DataAnnotations;

namespace Athlo.Models.DTOs.Workouts;

public class CompleteWorkoutRequest
{
    [Required]
    public Guid SessionId { get; set; }

    public int CaloriesBurned { get; set; }
}
