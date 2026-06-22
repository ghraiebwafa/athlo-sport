using System.ComponentModel.DataAnnotations;
using Athlo.Shared.Enums;

namespace Athlo.Models.DTOs.Auth;

public class UpdateProfileRequest
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    public decimal? CurrentWeight { get; set; }
    public decimal? GoalWeight { get; set; }
    public FitnessGoal? FitnessGoal { get; set; }
}
