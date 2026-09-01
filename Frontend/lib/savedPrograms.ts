import {
  getSavedProgramStatus,
  getSavedPrograms,
  saveProgram,
  unsaveProgram,
} from '@/lib/api/programs';
import type { ProgramListItem } from '@/lib/types';

export async function getSavedProgramIds(): Promise<string[]> {
  const programs = await getSavedPrograms();
  return programs.map((p) => p.id);
}

export async function listSavedPrograms(): Promise<ProgramListItem[]> {
  return getSavedPrograms();
}

export async function isProgramSaved(id: string): Promise<boolean> {
  const status = await getSavedProgramStatus(id);
  return status.saved;
}

export async function toggleSavedProgram(id: string): Promise<boolean> {
  const status = await getSavedProgramStatus(id);
  if (status.saved) {
    await unsaveProgram(id);
    return false;
  }
  await saveProgram(id);
  return true;
}
