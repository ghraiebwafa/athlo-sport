import { getItem, setItem } from '@/lib/storage';
import type { WorkoutSetLog } from '@/lib/types';

const KEY = 'athlo_pending_workout_sets';

export interface PendingWorkoutSet {
  id: string;
  sessionId: string;
  programExerciseId: string;
  exerciseId: string;
  exerciseName: string;
  setNumber: number;
  repsCompleted: number;
  weightKg?: number;
  createdAt: string;
}

export type PendingWorkoutSetInput = Omit<PendingWorkoutSet, 'id' | 'createdAt'>;

function createPendingId() {
  return `pending-${Date.now()}-${Math.random().toString(36).slice(2, 9)}`;
}

async function readAll(): Promise<PendingWorkoutSet[]> {
  const raw = await getItem(KEY);
  if (!raw) return [];
  try {
    return JSON.parse(raw) as PendingWorkoutSet[];
  } catch {
    return [];
  }
}

async function writeAll(entries: PendingWorkoutSet[]) {
  await setItem(KEY, JSON.stringify(entries));
}

export async function getPendingSets(sessionId: string): Promise<PendingWorkoutSet[]> {
  const all = await readAll();
  return all.filter((entry) => entry.sessionId === sessionId);
}

export async function enqueuePendingSet(input: PendingWorkoutSetInput): Promise<PendingWorkoutSet> {
  const entry: PendingWorkoutSet = {
    ...input,
    id: createPendingId(),
    createdAt: new Date().toISOString(),
  };
  const all = await readAll();
  all.push(entry);
  await writeAll(all);
  return entry;
}

export async function removePendingSet(id: string) {
  const all = await readAll();
  await writeAll(all.filter((entry) => entry.id !== id));
}

export function toOptimisticSetLog(entry: PendingWorkoutSet): WorkoutSetLog {
  return {
    id: `pending:${entry.id}`,
    programExerciseId: entry.programExerciseId,
    exerciseId: entry.exerciseId,
    exerciseName: entry.exerciseName,
    setNumber: entry.setNumber,
    repsCompleted: entry.repsCompleted,
    weightKg: entry.weightKg,
    completed: true,
    loggedAt: entry.createdAt,
  };
}

export async function flushPendingSets(
  sessionId: string,
  logFn: (payload: {
    programExerciseId: string;
    setNumber: number;
    repsCompleted: number;
    weightKg?: number;
  }) => Promise<unknown>
): Promise<{ synced: number; failed: number }> {
  const pending = await getPendingSets(sessionId);
  let synced = 0;
  let failed = 0;

  for (const entry of pending) {
    try {
      await logFn({
        programExerciseId: entry.programExerciseId,
        setNumber: entry.setNumber,
        repsCompleted: entry.repsCompleted,
        weightKg: entry.weightKg,
      });
      await removePendingSet(entry.id);
      synced += 1;
    } catch {
      failed += 1;
      break;
    }
  }

  return { synced, failed };
}
