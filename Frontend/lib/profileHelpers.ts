import type { FitnessGoal, ProgressData, UserProfile } from '@/lib/types';

export function formatFitnessGoal(goal: FitnessGoal, current: number, target: number): string {
  switch (goal) {
    case 'LoseWeight':
      return `Lose ${Math.max(0, current - target).toFixed(1)} kg`;
    case 'BuildMuscle':
      return `Gain ${Math.max(0, target - current).toFixed(1)} kg`;
    default:
      return 'Stay Active';
  }
}

export function kgToGo(current: number, target: number, goal: FitnessGoal): number {
  if (goal === 'BuildMuscle') return Math.max(0, target - current);
  if (goal === 'LoseWeight') return Math.max(0, current - target);
  return 0;
}

export interface Achievement {
  id: string;
  title: string;
  subtitle: string;
  color: string;
  unlocked: boolean;
}

export function buildAchievements(progress: ProgressData): Achievement[] {
  return [
    {
      id: 'first',
      title: 'First Workout',
      subtitle: 'Completed',
      color: '#007AFF',
      unlocked: progress.totalWorkouts >= 1,
    },
    {
      id: 'streak7',
      title: '7 Day Streak',
      subtitle: 'Achieved',
      color: '#FF9500',
      unlocked: progress.currentStreak >= 7,
    },
    {
      id: 'workouts25',
      title: '25 Workouts',
      subtitle: 'Completed',
      color: '#34C759',
      unlocked: progress.totalWorkouts >= 25,
    },
    {
      id: 'calories10k',
      title: '10K Calories',
      subtitle: 'Burned',
      color: '#AF52DE',
      unlocked: progress.totalCaloriesBurned >= 10000,
    },
  ];
}

export interface PersonalRecord {
  id: string;
  label: string;
  value: string;
  color: string;
}

const PR_COLORS = ['#007AFF', '#FF9500', '#34C759', '#AF52DE', '#FF2D55'];

export function buildPersonalRecords(progress: ProgressData): PersonalRecord[] {
  const liftRecords = progress.personalRecords ?? [];
  if (liftRecords.length > 0) {
    return liftRecords.slice(0, 5).map((record, index) => ({
      id: record.exerciseId,
      label: record.exerciseName,
      value: `${formatWeight(record.weightKg)} kg × ${record.reps}`,
      color: PR_COLORS[index % PR_COLORS.length],
    }));
  }

  const workouts = progress.recentWorkouts;
  const longest = workouts.reduce((max, w) => Math.max(max, w.durationMinutes), 0);
  const mostCal = workouts.reduce((max, w) => Math.max(max, w.caloriesBurned), 0);

  return [
    { id: 'longest', label: 'Longest Workout', value: `${longest || 0} min`, color: '#007AFF' },
    { id: 'calories', label: 'Most Calories Burned', value: `${mostCal || 0} kcal`, color: '#FF9500' },
    { id: 'streak', label: 'Current Streak', value: `${progress.currentStreak} days`, color: '#34C759' },
  ];
}

function formatWeight(kg: number): string {
  return Number.isInteger(kg) ? String(kg) : kg.toFixed(1);
}

export function memberLabel(role: UserProfile['role']): string {
  if (role === 'SuperAdmin') return 'Super Admin';
  if (role === 'Admin') return 'Admin Member';
  return 'Member';
}
