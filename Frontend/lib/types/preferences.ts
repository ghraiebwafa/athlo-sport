import type { HeartRateSource, RestPresetSeconds } from '@/stores/preferencesStore';

export interface UserPreferencesResponse {
  notifyWorkoutReminders: boolean;
  notifyPrAlerts: boolean;
  notifyStreakReminders: boolean;
  pushPermissionAsked: boolean;
  heartRateSource: HeartRateSource;
  defaultRestSeconds: RestPresetSeconds;
  betweenExerciseRestSeconds: RestPresetSeconds;
}
