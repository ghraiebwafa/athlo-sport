using Athlo.Mapper;
using Athlo.Shared.Enums;

namespace Athlo.Tests;

public class UserMapperTests
{
    [Fact]
    public void CalculateGoalProgress_LoseWeight_ReturnsExpectedPercent()
    {
        var progress = UserMapper.CalculateGoalProgress(72m, 70m, 67m, FitnessGoal.LoseWeight);
        Assert.True(progress > 0);
        Assert.True(progress <= 100);
    }

    [Fact]
    public void CalculateGoalProgress_AtGoal_Returns100()
    {
        var progress = UserMapper.CalculateGoalProgress(72m, 67m, 67m, FitnessGoal.LoseWeight);
        Assert.Equal(100, progress);
    }
}
