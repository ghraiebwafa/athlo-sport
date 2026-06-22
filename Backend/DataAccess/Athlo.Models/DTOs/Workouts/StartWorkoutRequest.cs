using System.ComponentModel.DataAnnotations;

namespace Athlo.Models.DTOs.Workouts;

public class StartWorkoutRequest
{
    [Required]
    public Guid ProgramId { get; set; }
}
