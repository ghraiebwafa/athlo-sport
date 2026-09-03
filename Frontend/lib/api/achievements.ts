import { managementApi } from '@/lib/api/client';

export interface AchievementDto {
  key: string;
  title: string;
  subtitle: string;
  color: string;
  unlocked: boolean;
  unlockedAt?: string;
}

export async function getAchievements() {
  const { data } = await managementApi.get<AchievementDto[]>('/api/achievements');
  return data;
}
