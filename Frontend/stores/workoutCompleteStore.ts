import { getItem, removeItem, setItem } from '@/lib/storage';
import { create } from 'zustand';
import type { ProgramExercise } from '@/lib/types';

const STORAGE_KEY = 'athlo_workout_summary';

export interface WorkoutCompleteSummary {
  sessionId: string;
  programName: string;
  durationSeconds: number;
  caloriesBurned: number;
  avgHeartRate: number;
  intensityPercent: number;
  exercises: ProgramExercise[];
  completedAt: string;
}

interface WorkoutCompleteState {
  summary: WorkoutCompleteSummary | null;
  setSummary: (summary: WorkoutCompleteSummary) => Promise<void>;
  clearSummary: () => Promise<void>;
  hydrateSummary: () => Promise<WorkoutCompleteSummary | null>;
}

export const useWorkoutCompleteStore = create<WorkoutCompleteState>((set) => ({
  summary: null,

  setSummary: async (summary) => {
    set({ summary });
    await setItem(STORAGE_KEY, JSON.stringify(summary));
  },

  clearSummary: async () => {
    set({ summary: null });
    await removeItem(STORAGE_KEY);
  },

  hydrateSummary: async () => {
    const raw = await getItem(STORAGE_KEY);
    if (!raw) return null;

    try {
      const summary = JSON.parse(raw) as WorkoutCompleteSummary;
      set({ summary });
      return summary;
    } catch {
      await removeItem(STORAGE_KEY);
      return null;
    }
  },
}));
