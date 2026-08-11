import { authApi, managementApi } from '@/lib/api/client';
import type {
  AdminDashboardStats,
  AdminUserListItem,
  Category,
  CreateProgramPayload,
  Exercise,
  PagedResult,
  ProgramDetail,
} from '@/lib/types';

export async function getExercises() {
  const { data } = await managementApi.get<Exercise[]>('/api/exercises');
  return data;
}

export async function createExercise(payload: { name: string; imageUrl?: string }) {
  const { data } = await managementApi.post<Exercise>('/api/admin/exercises', payload);
  return data;
}

export async function updateExercise(id: string, payload: { name: string; imageUrl?: string }) {
  const { data } = await managementApi.put<Exercise>(`/api/admin/exercises/${id}`, payload);
  return data;
}

export async function deleteExercise(id: string) {
  await managementApi.delete(`/api/admin/exercises/${id}`);
}

export async function createCategory(payload: { name: string; slug: string; icon: string }) {
  const { data } = await managementApi.post<Category>('/api/admin/categories', payload);
  return data;
}

export async function updateCategory(
  id: string,
  payload: { name: string; slug: string; icon: string }
) {
  const { data } = await managementApi.put<Category>(`/api/admin/categories/${id}`, payload);
  return data;
}

export async function deleteCategory(id: string) {
  await managementApi.delete(`/api/admin/categories/${id}`);
}

export async function createProgram(payload: CreateProgramPayload) {
  const { data } = await managementApi.post<ProgramDetail>('/api/admin/programs', payload);
  return data;
}

export async function updateProgram(id: string, payload: CreateProgramPayload) {
  const { data } = await managementApi.put<ProgramDetail>(`/api/admin/programs/${id}`, payload);
  return data;
}

export async function deleteProgram(id: string) {
  await managementApi.delete(`/api/admin/programs/${id}`);
}

export async function getAdminUsers(page = 1, pageSize = 20) {
  const { data } = await authApi.get<PagedResult<AdminUserListItem>>('/api/admin/users', {
    params: { page, pageSize },
  });
  return data;
}

export async function getAdminStats() {
  const { data } = await managementApi.get<AdminDashboardStats>('/api/admin/stats');
  return data;
}

export async function getAdmins() {
  const { data } = await authApi.get<AdminUserListItem[]>('/api/admin/admins');
  return data;
}

export async function createAdmin(payload: {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
}) {
  const { data } = await authApi.post<AdminUserListItem>('/api/admin/admins', payload);
  return data;
}

export async function removeAdmin(id: string) {
  await authApi.delete(`/api/admin/admins/${id}`);
}
