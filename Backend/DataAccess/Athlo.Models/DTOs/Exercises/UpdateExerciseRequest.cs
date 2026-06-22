namespace Athlo.Models.DTOs.Exercises;

public class UpdateExerciseRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
