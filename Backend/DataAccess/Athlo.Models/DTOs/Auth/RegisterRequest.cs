using System.ComponentModel.DataAnnotations;
using Athlo.Shared.Enums;

namespace Athlo.Models.DTOs.Auth;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string ConfirmPassword { get; set; } = string.Empty;

    public FitnessGoal FitnessGoal { get; set; } = FitnessGoal.StayActive;

    [Range(20, 500)]
    public decimal CurrentWeight { get; set; }

    [Range(20, 500)]
    public decimal GoalWeight { get; set; }
}
