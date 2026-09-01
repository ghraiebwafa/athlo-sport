import * as SecureStore from 'expo-secure-store';
import { create } from 'zustand';
import { DEFAULT_REST_SECONDS } from '@/components/workout/RestTimer';

const STORAGE_KEY = 'athlo_user_prefs_v1';

export type HeartRateSource = 'estimated' | 'manual';

export const REST_PRESET_SECONDS = [60, 90, 120] as const;
export type RestPresetSeconds = (typeof REST_PRESET_SECONDS)[number];

export interface UserPreferences {
  notifyWorkoutReminders: boolean;
  notifyPrAlerts: boolean;
  notifyStreakReminders: boolean;
  pushPermissionAsked: boolean;
  heartRateSource: HeartRateSource;
  defaultRestSeconds: RestPresetSeconds;
  betweenExerciseRestSeconds: RestPresetSeconds;
}

const defaults: UserPreferences = {
  notifyWorkoutReminders: true,
  notifyPrAlerts: true,
  notifyStreakReminders: false,
  pushPermissionAsked: false,
  heartRateSource: 'estimated',
  defaultRestSeconds: DEFAULT_REST_SECONDS,
  betweenExerciseRestSeconds: 120,
};

interface PreferencesState extends UserPreferences {
  hydrated: boolean;
  hydrate: () => Promise<void>;
  setPreference: <K extends keyof UserPreferences>(key: K, value: UserPreferences[K]) => Promise<void>;
}

async function persist(prefs: UserPreferences) {
  await SecureStore.setItemAsync(STORAGE_KEY, JSON.stringify(prefs));
}

function coerceRestPreset(value: unknown, fallback: RestPresetSeconds): RestPresetSeconds {
  return REST_PRESET_SECONDS.includes(value as RestPresetSeconds)
    ? (value as RestPresetSeconds)
    : fallback;
}

export const usePreferencesStore = create<PreferencesState>((set, get) => ({
  ...defaults,
  hydrated: false,
  hydrate: async () => {
    try {
      const raw = await SecureStore.getItemAsync(STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw) as Partial<UserPreferences>;
        set({
          ...defaults,
          ...parsed,
          defaultRestSeconds: coerceRestPreset(parsed.defaultRestSeconds, defaults.defaultRestSeconds),
          betweenExerciseRestSeconds: coerceRestPreset(
            parsed.betweenExerciseRestSeconds,
            defaults.betweenExerciseRestSeconds
          ),
          hydrated: true,
        });
        return;
      }
    } catch {
      // fall through to defaults
    }
    set({ hydrated: true });
  },
  setPreference: async (key, value) => {
    const next = {
      notifyWorkoutReminders: get().notifyWorkoutReminders,
      notifyPrAlerts: get().notifyPrAlerts,
      notifyStreakReminders: get().notifyStreakReminders,
      pushPermissionAsked: get().pushPermissionAsked,
      heartRateSource: get().heartRateSource,
      defaultRestSeconds: get().defaultRestSeconds,
      betweenExerciseRestSeconds: get().betweenExerciseRestSeconds,
      [key]: value,
    } satisfies UserPreferences;
    set(next);
    await persist(next);
  },
}));
