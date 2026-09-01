import { managementApi } from '@/lib/api/client';
import type { Category, ProgramDetail, ProgramListItem } from '@/lib/types';

export async function getCategories() {
  const { data } = await managementApi.get<Category[]>('/api/programs/categories');
  return data;
}

export async function getPrograms() {
  const { data } = await managementApi.get<ProgramListItem[]>('/api/programs');
  return data;
}

export async function getProgram(id: string) {
  const { data } = await managementApi.get<ProgramDetail>(`/api/programs/${id}`);
  return data;
}

export async function getSavedPrograms() {
  const { data } = await managementApi.get<ProgramListItem[]>('/api/programs/saved');
  return data;
}

export async function getSavedProgramStatus(programId: string) {
  const { data } = await managementApi.get<{ programId: string; saved: boolean }>(
    `/api/programs/saved/${programId}`
  );
  return data;
}

export async function saveProgram(programId: string) {
  await managementApi.post(`/api/programs/saved/${programId}`);
}

export async function unsaveProgram(programId: string) {
  await managementApi.delete(`/api/programs/saved/${programId}`);
}
