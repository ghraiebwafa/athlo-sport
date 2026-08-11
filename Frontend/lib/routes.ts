import type { Href } from 'expo-router';

export const ROUTES = {
  home: '/(protected)/(tabs)' as Href,
  programs: '/(protected)/(tabs)/programs' as Href,
  progress: '/(protected)/(tabs)/progress' as Href,
  profile: '/(protected)/(tabs)/profile' as Href,
  login: '/(auth)/login' as Href,
  register: '/(auth)/register' as Href,
  forgotPassword: '/(auth)/forgot-password' as Href,
  onboarding: '/onboarding' as Href,
  activeWorkout: '/(protected)/workout/active' as Href,
  completeWorkout: '/(protected)/workout/complete' as Href,
  editProfile: '/(protected)/profile/edit' as Href,
  changePassword: '/(protected)/profile/change-password' as Href,
  editGoals: '/(protected)/profile/edit-goals' as Href,
  workoutHistory: '/(protected)/workout/history' as Href,
  savedPrograms: '/(protected)/programs/saved' as Href,
} as const;

export function programDetail(id: string): Href {
  return `/(protected)/program/${id}` as Href;
}
