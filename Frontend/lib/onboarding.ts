import { getItem, setItem } from '@/lib/storage';

const KEY = 'athlo_onboarding_done';

export async function hasCompletedOnboarding(): Promise<boolean> {
  const value = await getItem(KEY);
  return value === '1';
}

export async function setOnboardingComplete(): Promise<void> {
  await setItem(KEY, '1');
}
