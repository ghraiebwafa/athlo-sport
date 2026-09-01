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
  achievements: '/(protected)/profile/achievements' as Href,
  privacy: '/(protected)/profile/privacy' as Href,
  help: '/(protected)/profile/help' as Href,
  personalRecords: '/(protected)/profile/personal-records' as Href,
  notifications: '/(protected)/profile/notifications' as Href,
  devices: '/(protected)/profile/devices' as Href,
  adminAdmins: '/(protected)/admin/admins' as Href,
} as const;

export function programDetail(id: string): Href {
  return `/(protected)/program/${id}` as Href;
}

export function workoutHistoryDetail(id: string): Href {
  return `/(protected)/workout/history/${id}` as Href;
}

export function adminProgramEdit(id: string): Href {
  return `/(protected)/admin/programs/${id}` as Href;
}

export function adminProgramCreate(): Href {
  return '/(protected)/admin/programs/new' as Href;
}
