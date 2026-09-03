import { managementApi } from '@/lib/api/client';
import type { ProgressData } from '@/lib/types';

export interface WeeklySummaryDto {
  weekStart: string;
  weekEnd: string;
  workoutsCompleted: number;
  caloriesBurned: number;
  currentStreak: number;
  minutesTrained: number;
  headline: string;
}

export async function getProgress() {
  const { data } = await managementApi.get<ProgressData>('/api/progress');
  return data;
}

export async function getWeeklySummary() {
  const { data } = await managementApi.get<WeeklySummaryDto>('/api/progress/weekly-summary');
  return data;
}
