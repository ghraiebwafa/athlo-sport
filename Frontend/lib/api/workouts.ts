import { managementApi } from '@/lib/api/client';
import type { PagedResult, WorkoutSession, WorkoutSetLog } from '@/lib/types';

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

export async function pauseWorkout(sessionId: string) {
  const { data } = await managementApi.post<WorkoutSession>(`/api/workouts/${sessionId}/pause`);
  return data;
}

export async function resumeWorkout(sessionId: string) {
  const { data } = await managementApi.post<WorkoutSession>(`/api/workouts/${sessionId}/resume`);
  return data;
}

export async function logWorkoutSet(
  sessionId: string,
  payload: {
    programExerciseId: string;
    setNumber: number;
    repsCompleted: number;
    weightKg?: number;
    completed?: boolean;
  }
) {
  const { data } = await managementApi.post<WorkoutSetLog>(`/api/workouts/${sessionId}/sets`, {
    ...payload,
    completed: payload.completed ?? true,
  });
  return data;
}

export async function updateWorkoutSet(
  setLogId: string,
  payload: { repsCompleted: number; weightKg?: number; completed?: boolean }
) {
  const { data } = await managementApi.put<WorkoutSetLog>(`/api/workouts/sets/${setLogId}`, {
    ...payload,
    completed: payload.completed ?? true,
  });
  return data;
}

export async function getWorkoutHistory(page = 1, pageSize = 20) {
  const { data } = await managementApi.get<PagedResult<WorkoutSession>>('/api/workouts/history', {
    params: { page, pageSize },
  });
  return data;
}
