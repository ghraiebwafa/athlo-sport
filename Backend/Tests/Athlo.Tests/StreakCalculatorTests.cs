using Athlo.Shared.Helpers;

namespace Athlo.Tests;

public class StreakCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsZero_WhenNoWorkouts()
    {
        var streak = StreakCalculator.Calculate([]);
        Assert.Equal(0, streak);
    }

    [Fact]
    public void Calculate_ReturnsConsecutiveDays()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dates = new[] { today, today.AddDays(-1), today.AddDays(-2) };
        var streak = StreakCalculator.Calculate(dates);
        Assert.Equal(3, streak);
    }
}
