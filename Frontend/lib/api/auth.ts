import { authApi } from '@/lib/api/client';
import type { UserPreferencesResponse } from '@/lib/types/preferences';
import type { AuthResponse, FitnessGoal, UserProfile } from '@/lib/types';

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  fitnessGoal: FitnessGoal;
  currentWeight: number;
  goalWeight: number;
}

export async function login(payload: LoginPayload) {
  const { data } = await authApi.post<AuthResponse>('/api/auth/login', payload);
  return data;
}

export async function register(payload: RegisterPayload) {
  const { data } = await authApi.post<AuthResponse>('/api/auth/register', payload);
  return data;
}

export async function getProfile() {
  const { data } = await authApi.get<UserProfile>('/api/auth/profile');
  return data;
}

export interface UpdateProfilePayload {
  fullName?: string;
  currentWeight?: number;
  goalWeight?: number;
  fitnessGoal?: FitnessGoal;
}

export async function updateProfile(payload: UpdateProfilePayload) {
  const { data } = await authApi.put<UserProfile>('/api/auth/profile', payload);
  return data;
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export async function changePassword(payload: ChangePasswordPayload) {
  await authApi.post('/api/auth/change-password', payload);
}

export async function forgotPassword(email: string) {
  const { data } = await authApi.post<{ message: string; resetToken?: string }>(
    '/api/auth/forgot-password',
    { email }
  );
  return data;
}

export async function resetPassword(token: string, newPassword: string, confirmNewPassword: string) {
  await authApi.post('/api/auth/reset-password', { token, newPassword, confirmNewPassword });
}

export async function logout(refreshToken: string) {
  await authApi.post('/api/auth/logout', { refreshToken });
}

export async function getUserPreferences() {
  const { data } = await authApi.get<UserPreferencesResponse>('/api/auth/preferences');
  return data;
}

export async function updateUserPreferences(preferences: UserPreferencesResponse) {
  const { data } = await authApi.put<UserPreferencesResponse>('/api/auth/preferences', preferences);
  return data;
}
