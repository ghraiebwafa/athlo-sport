export function getGreeting(): string {
  const hour = new Date().getHours();
  if (hour < 12) return 'Good Morning';
  if (hour < 17) return 'Good Afternoon';
  return 'Good Evening';
}

export function formatLongDate(date = new Date()): string {
  return date.toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric' });
}

export function firstName(fullName: string): string {
  return fullName.trim().split(/\s+/)[0] ?? fullName;
}

export function caloriesToday(
  workouts: { completedAt: string; caloriesBurned: number }[]
): number {
  const today = new Date().toDateString();
  return workouts
    .filter((w) => new Date(w.completedAt).toDateString() === today)
    .reduce((sum, w) => sum + w.caloriesBurned, 0);
}

export function workoutsThisWeek(weeklyFrequency: { weekStart: string; workoutCount: number }[]): number {
  if (weeklyFrequency.length === 0) return 0;
  const latest = weeklyFrequency[weeklyFrequency.length - 1];
  return latest?.workoutCount ?? 0;
}
