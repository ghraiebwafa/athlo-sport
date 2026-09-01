namespace Athlo.Models.Entities;

public class SavedProgram
{
    public Guid UserId { get; set; }
    public Guid ProgramId { get; set; }
    public DateTime SavedAt { get; set; }

    public User User { get; set; } = null!;
    public WorkoutProgram Program { get; set; } = null!;
}
