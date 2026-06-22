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
