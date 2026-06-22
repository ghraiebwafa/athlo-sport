using Athlo.Models.DTOs.Auth;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;

namespace Athlo.Mapper;

public static class UserMapper
{
    public static UserProfileResponse ToProfileResponse(User user) =>
        new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            CurrentWeight = user.CurrentWeight,
            GoalWeight = user.GoalWeight,
            FitnessGoal = user.FitnessGoal,
            Role = user.Role,
            GoalProgressPercent = CalculateGoalProgress(user.InitialWeight, user.CurrentWeight, user.GoalWeight, user.FitnessGoal)
        };

    public static double CalculateGoalProgress(
        decimal initialWeight,
        decimal currentWeight,
        decimal goalWeight,
        FitnessGoal fitnessGoal)
    {
        if (initialWeight <= 0 || goalWeight <= 0)
            return 0;

        if (currentWeight == goalWeight)
            return 100;

        var totalDelta = goalWeight - initialWeight;
        var currentDelta = currentWeight - initialWeight;

        if (totalDelta == 0)
            return currentWeight == goalWeight ? 100 : 0;

        // Progress only counts when moving toward the goal direction
        var movingTowardGoal = fitnessGoal switch
        {
            FitnessGoal.LoseWeight => currentDelta <= 0 && totalDelta < 0,
            FitnessGoal.BuildMuscle => currentDelta >= 0 && totalDelta > 0,
            _ => Math.Abs(currentWeight - goalWeight) < Math.Abs(initialWeight - goalWeight)
        };

        if (!movingTowardGoal && currentWeight != goalWeight)
            return 0;

        var progress = Math.Abs((double)(currentDelta / totalDelta)) * 100;
        return Math.Round(Math.Clamp(progress, 0, 100), 1);
    }
}
