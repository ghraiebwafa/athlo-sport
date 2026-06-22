import { managementApi } from '@/lib/api/client';
import type { WorkoutSession } from '@/lib/types';

export async function getActiveWorkout(): Promise<WorkoutSession | null> {
  const response = await managementApi.get<WorkoutSession>('/api/workouts/active');
  return response.status === 204 ? null : response.data;
}

export async function startWorkout(programId: string) {
  const { data } = await managementApi.post<WorkoutSession>('/api/workouts/start', { programId });
  return data;
}

export async function completeWorkout(sessionId: string, caloriesBurned: number) {
  const { data } = await managementApi.post<WorkoutSession>('/api/workouts/complete', {
    sessionId,
    caloriesBurned,
  });
  return data;
}

export async function cancelWorkout(sessionId: string) {
  const { data } = await managementApi.post<WorkoutSession>('/api/workouts/cancel', { sessionId });
  return data;
}
