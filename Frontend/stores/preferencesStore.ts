import * as SecureStore from 'expo-secure-store';
import { create } from 'zustand';

const STORAGE_KEY = 'athlo_user_prefs_v1';

export type HeartRateSource = 'estimated' | 'manual';

export interface UserPreferences {
  notifyWorkoutReminders: boolean;
  notifyPrAlerts: boolean;
  notifyStreakReminders: boolean;
  pushPermissionAsked: boolean;
  heartRateSource: HeartRateSource;
}

const defaults: UserPreferences = {
  notifyWorkoutReminders: true,
  notifyPrAlerts: true,
  notifyStreakReminders: false,
  pushPermissionAsked: false,
  heartRateSource: 'estimated',
};

interface PreferencesState extends UserPreferences {
  hydrated: boolean;
  hydrate: () => Promise<void>;
  setPreference: <K extends keyof UserPreferences>(key: K, value: UserPreferences[K]) => Promise<void>;
}

async function persist(prefs: UserPreferences) {
  await SecureStore.setItemAsync(STORAGE_KEY, JSON.stringify(prefs));
}

export const usePreferencesStore = create<PreferencesState>((set, get) => ({
  ...defaults,
  hydrated: false,
  hydrate: async () => {
    try {
      const raw = await SecureStore.getItemAsync(STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw) as Partial<UserPreferences>;
        set({ ...defaults, ...parsed, hydrated: true });
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
      [key]: value,
    } satisfies UserPreferences;
    set(next);
    await persist(next);
  },
}));
