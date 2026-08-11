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
  admin: '/(protected)/admin' as Href,
  adminExercises: '/(protected)/admin/exercises' as Href,
  adminCategories: '/(protected)/admin/categories' as Href,
  adminPrograms: '/(protected)/admin/programs' as Href,
  adminUsers: '/(protected)/admin/users' as Href,
} as const;

export function programDetail(id: string): Href {
  return `/(protected)/program/${id}` as Href;
}

export function adminProgramEdit(id: string): Href {
  return `/(protected)/admin/programs/${id}` as Href;
}

export function adminProgramCreate(): Href {
  return '/(protected)/admin/programs/new' as Href;
}
