import { getItem, setItem } from '@/lib/storage';

const KEY = 'athlo_saved_programs';

export async function getSavedProgramIds(): Promise<string[]> {
  const raw = await getItem(KEY);
  if (!raw) return [];
  try {
    return JSON.parse(raw) as string[];
  } catch {
    return [];
  }
}

export async function isProgramSaved(id: string): Promise<boolean> {
  const ids = await getSavedProgramIds();
  return ids.includes(id);
}

export async function toggleSavedProgram(id: string): Promise<boolean> {
  const ids = await getSavedProgramIds();
  const exists = ids.includes(id);
  const next = exists ? ids.filter((x) => x !== id) : [...ids, id];
  await setItem(KEY, JSON.stringify(next));
  return !exists;
}
