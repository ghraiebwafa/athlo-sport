using System.ComponentModel.DataAnnotations;

namespace Athlo.Models.DTOs.Workouts;

public class CancelWorkoutRequest
{
    [Required]
    public Guid SessionId { get; set; }
}
