import { getItem, setItem } from '@/lib/storage';
import type { WorkoutSetLog } from '@/lib/types';

const LOGS_KEY = 'athlo_pending_workout_sets';
const UPDATES_KEY = 'athlo_pending_workout_set_updates';

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

export interface PendingSetUpdate {
  id: string;
  sessionId: string;
  setLogId: string;
  repsCompleted: number;
  weightKg?: number;
  createdAt: string;
}

export type PendingWorkoutSetInput = Omit<PendingWorkoutSet, 'id' | 'createdAt'>;
export type PendingSetUpdateInput = Omit<PendingSetUpdate, 'id' | 'createdAt'>;

function createPendingId() {
  return `pending-${Date.now()}-${Math.random().toString(36).slice(2, 9)}`;
}

async function readLogs(): Promise<PendingWorkoutSet[]> {
  const raw = await getItem(LOGS_KEY);
  if (!raw) return [];
  try {
    return JSON.parse(raw) as PendingWorkoutSet[];
  } catch {
    return [];
  }
}

async function writeLogs(entries: PendingWorkoutSet[]) {
  await setItem(LOGS_KEY, JSON.stringify(entries));
}

async function readUpdates(): Promise<PendingSetUpdate[]> {
  const raw = await getItem(UPDATES_KEY);
  if (!raw) return [];
  try {
    return JSON.parse(raw) as PendingSetUpdate[];
  } catch {
    return [];
  }
}

async function writeUpdates(entries: PendingSetUpdate[]) {
  await setItem(UPDATES_KEY, JSON.stringify(entries));
}

export async function getPendingSets(sessionId: string): Promise<PendingWorkoutSet[]> {
  const all = await readLogs();
  return all.filter((entry) => entry.sessionId === sessionId);
}

export async function getPendingSetUpdates(sessionId: string): Promise<PendingSetUpdate[]> {
  const all = await readUpdates();
  return all.filter((entry) => entry.sessionId === sessionId);
}

export async function enqueuePendingSet(input: PendingWorkoutSetInput): Promise<PendingWorkoutSet> {
  const entry: PendingWorkoutSet = {
    ...input,
    id: createPendingId(),
    createdAt: new Date().toISOString(),
  };
  const all = await readLogs();
  all.push(entry);
  await writeLogs(all);
  return entry;
}

export async function enqueuePendingSetUpdate(input: PendingSetUpdateInput): Promise<PendingSetUpdate> {
  const all = await readUpdates();
  const existing = all.find((entry) => entry.setLogId === input.setLogId);
  if (existing) {
    existing.repsCompleted = input.repsCompleted;
    existing.weightKg = input.weightKg;
    existing.createdAt = new Date().toISOString();
    await writeUpdates(all);
    return existing;
  }

  const entry: PendingSetUpdate = {
    ...input,
    id: createPendingId(),
    createdAt: new Date().toISOString(),
  };
  all.push(entry);
  await writeUpdates(all);
  return entry;
}

export async function removePendingSet(id: string) {
  const all = await readLogs();
  await writeLogs(all.filter((entry) => entry.id !== id));
}

export async function removePendingSetUpdate(id: string) {
  const all = await readUpdates();
  await writeUpdates(all.filter((entry) => entry.id !== id));
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

export function applyOptimisticSetUpdate(
  sets: WorkoutSetLog[],
  update: PendingSetUpdate
): WorkoutSetLog[] {
  return sets.map((set) =>
    set.id === update.setLogId
      ? {
          ...set,
          repsCompleted: update.repsCompleted,
          weightKg: update.weightKg,
          loggedAt: update.createdAt,
        }
      : set
  );
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

export async function flushPendingSetUpdates(
  sessionId: string,
  updateFn: (payload: {
    setLogId: string;
    repsCompleted: number;
    weightKg?: number;
  }) => Promise<unknown>
): Promise<{ synced: number; failed: number }> {
  const pending = await getPendingSetUpdates(sessionId);
  let synced = 0;
  let failed = 0;

  for (const entry of pending) {
    try {
      await updateFn({
        setLogId: entry.setLogId,
        repsCompleted: entry.repsCompleted,
        weightKg: entry.weightKg,
      });
      await removePendingSetUpdate(entry.id);
      synced += 1;
    } catch {
      failed += 1;
      break;
    }
  }

  return { synced, failed };
}

export async function flushAllPendingWorkoutChanges(
  sessionId: string,
  handlers: {
    logSet: (payload: {
      programExerciseId: string;
      setNumber: number;
      repsCompleted: number;
      weightKg?: number;
    }) => Promise<unknown>;
    updateSet: (payload: {
      setLogId: string;
      repsCompleted: number;
      weightKg?: number;
    }) => Promise<unknown>;
  }
): Promise<{ synced: number; failed: number }> {
  const logs = await flushPendingSets(sessionId, handlers.logSet);
  if (logs.failed > 0) return logs;

  const updates = await flushPendingSetUpdates(sessionId, handlers.updateSet);
  return {
    synced: logs.synced + updates.synced,
    failed: updates.failed,
  };
}

export async function hasPendingWorkoutChanges(sessionId: string): Promise<boolean> {
  const [logs, updates] = await Promise.all([
    getPendingSets(sessionId),
    getPendingSetUpdates(sessionId),
  ]);
  return logs.length > 0 || updates.length > 0;
}
