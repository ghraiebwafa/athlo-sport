import type { ProgressData } from '@/lib/types';

export type TimeRange = '1W' | '1M' | '3M' | '6M' | '1Y' | 'All';

export const TIME_RANGES: TimeRange[] = ['1W', '1M', '3M', '6M', '1Y', 'All'];

function rangeDays(range: TimeRange): number | null {
  switch (range) {
    case '1W':
      return 7;
    case '1M':
      return 30;
    case '3M':
      return 90;
    case '6M':
      return 180;
    case '1Y':
      return 365;
    default:
      return null;
  }
}

function cutoffDate(range: TimeRange): Date | null {
  const days = rangeDays(range);
  if (days === null) return null;
  const d = new Date();
  d.setDate(d.getDate() - days);
  d.setHours(0, 0, 0, 0);
  return d;
}

export function filterWeeklyFrequency(
  weekly: ProgressData['weeklyFrequency'],
  range: TimeRange
) {
  const cutoff = cutoffDate(range);
  if (!cutoff) return weekly;
  return weekly.filter((w) => new Date(w.weekStart) >= cutoff);
}

export function filterRecentWorkouts(
  workouts: ProgressData['recentWorkouts'],
  range: TimeRange
) {
  const cutoff = cutoffDate(range);
  if (!cutoff) return workouts;
  return workouts.filter((w) => new Date(w.completedAt) >= cutoff);
}

export function totalDurationMinutes(workouts: ProgressData['recentWorkouts']): number {
  return workouts.reduce((sum, w) => sum + w.durationMinutes, 0);
}

export function formatDuration(totalMinutes: number): string {
  const h = Math.floor(totalMinutes / 60);
  const m = totalMinutes % 60;
  if (h === 0) return `${m}m`;
  return `${h}h ${m}m`;
}

export function trendPercent(current: number, previous: number): number | null {
  if (previous === 0) return current > 0 ? 100 : null;
  return Math.round(((current - previous) / previous) * 100);
}

export function splitWeeklyComparison(weekly: ProgressData['weeklyFrequency']) {
  if (weekly.length < 2) return { current: 0, previous: 0 };
  const current = weekly[weekly.length - 1]?.workoutCount ?? 0;
  const previous = weekly[weekly.length - 2]?.workoutCount ?? 0;
  return { current, previous };
}

/** Placeholder muscle split when API has no body-part data. */
export function muscleGroupBreakdown(programNames: string[]) {
  const groups = [
    { name: 'Back', color: '#007AFF', keywords: ['back', 'row', 'pull'] },
    { name: 'Chest', color: '#34C759', keywords: ['chest', 'push', 'press'] },
    { name: 'Shoulders', color: '#FF9500', keywords: ['shoulder', 'upper'] },
    { name: 'Arms', color: '#AF52DE', keywords: ['arm', 'bicep', 'tricep'] },
    { name: 'Legs', color: '#5AC8FA', keywords: ['leg', 'squat', 'lower'] },
  ];

  const counts = groups.map((g) => ({
    ...g,
    count: programNames.filter((n) => g.keywords.some((k) => n.toLowerCase().includes(k))).length,
  }));

  const total = counts.reduce((s, c) => s + c.count, 0) || 1;
  return counts.map((c) => ({
    name: c.name,
    color: c.color,
    percent: Math.max(8, Math.round((c.count / total) * 100)),
  }));
}

export function topMuscleGroup(breakdown: { name: string; percent: number }[]): string {
  return breakdown.reduce((a, b) => (b.percent > a.percent ? b : a)).name;
}
