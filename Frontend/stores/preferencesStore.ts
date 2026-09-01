import * as SecureStore from 'expo-secure-store';
import { create } from 'zustand';
import { DEFAULT_REST_SECONDS } from '@/components/workout/RestTimer';
import { getUserPreferences, updateUserPreferences } from '@/lib/api/auth';
import { getTokens } from '@/stores/authStore';

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
  syncFromServer: () => Promise<void>;
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

function mergePreferences(parsed: Partial<UserPreferences>): UserPreferences {
  return {
    ...defaults,
    ...parsed,
    defaultRestSeconds: coerceRestPreset(parsed.defaultRestSeconds, defaults.defaultRestSeconds),
    betweenExerciseRestSeconds: coerceRestPreset(
      parsed.betweenExerciseRestSeconds,
      defaults.betweenExerciseRestSeconds
    ),
    heartRateSource: parsed.heartRateSource === 'manual' ? 'manual' : 'estimated',
  };
}

function applyPreferences(prefs: UserPreferences) {
  usePreferencesStore.setState({ ...prefs, hydrated: true });
}

export const usePreferencesStore = create<PreferencesState>((set, get) => ({
  ...defaults,
  hydrated: false,
  hydrate: async () => {
    try {
      const raw = await SecureStore.getItemAsync(STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw) as Partial<UserPreferences>;
        set({ ...mergePreferences(parsed), hydrated: true });
        return;
      }
    } catch {
      // fall through to defaults
    }
    set({ hydrated: true });
  },
  syncFromServer: async () => {
    if (!getTokens()?.accessToken) return;

    try {
      const remote = await getUserPreferences();
      const merged = mergePreferences(remote);
      applyPreferences(merged);
      await persist(merged);
    } catch {
      // keep local preferences when offline or unauthenticated
    }
  },
  setPreference: async (key, value) => {
    const next = mergePreferences({
      notifyWorkoutReminders: get().notifyWorkoutReminders,
      notifyPrAlerts: get().notifyPrAlerts,
      notifyStreakReminders: get().notifyStreakReminders,
      pushPermissionAsked: get().pushPermissionAsked,
      heartRateSource: get().heartRateSource,
      defaultRestSeconds: get().defaultRestSeconds,
      betweenExerciseRestSeconds: get().betweenExerciseRestSeconds,
      [key]: value,
    });
    set(next);
    await persist(next);

    if (!getTokens()?.accessToken) return;

    try {
      await updateUserPreferences(next);
    } catch {
      // local save still applies; server sync retries on next syncFromServer
    }
  },
}));
