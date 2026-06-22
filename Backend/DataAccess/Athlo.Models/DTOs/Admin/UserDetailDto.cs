using Athlo.Shared.Enums;

namespace Athlo.Models.DTOs.Admin;

public class UserDetailDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public decimal CurrentWeight { get; set; }
    public decimal GoalWeight { get; set; }
    public FitnessGoal FitnessGoal { get; set; }
    public DateTime CreatedAt { get; set; }
}
