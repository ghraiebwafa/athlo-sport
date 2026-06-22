import { managementApi } from '@/lib/api/client';
import type { ProgressData } from '@/lib/types';

export async function getProgress() {
  const { data } = await managementApi.get<ProgressData>('/api/progress');
  return data;
}
