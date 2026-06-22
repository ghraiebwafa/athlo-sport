namespace Athlo.Models.Entities;

public class Exercise
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public ICollection<ProgramExercise> ProgramExercises { get; set; } = [];
}
