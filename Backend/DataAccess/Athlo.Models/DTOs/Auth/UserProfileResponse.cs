using Athlo.Shared.Enums;

namespace Athlo.Models.DTOs.Auth;

public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal CurrentWeight { get; set; }
    public decimal GoalWeight { get; set; }
    public FitnessGoal FitnessGoal { get; set; }
    public UserRole Role { get; set; }
    public double GoalProgressPercent { get; set; }
}
