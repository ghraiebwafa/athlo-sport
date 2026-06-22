namespace Athlo.Shared.Helpers;

public static class StreakCalculator
{
    /// <summary>
    /// Calculates consecutive-day workout streak.
    /// Allows a one-day grace period (yesterday counts if no workout today).
    /// </summary>
    public static int Calculate(IEnumerable<DateOnly> workoutDates)
    {
        var dates = workoutDates.Distinct().OrderByDescending(d => d).ToList();
        if (dates.Count == 0)
            return 0;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var mostRecent = dates[0];

        if (mostRecent < today.AddDays(-1))
            return 0;

        var streak = 1;
        var expected = mostRecent.AddDays(-1);

        for (var i = 1; i < dates.Count; i++)
        {
            if (dates[i] == expected)
            {
                streak++;
                expected = expected.AddDays(-1);
            }
            else if (dates[i] < expected)
            {
                break;
            }
        }

        return streak;
    }
}
