import type { UserRole } from '@/lib/types';

export function isAdminRole(role?: UserRole | null): boolean {
  return role === 'Admin' || role === 'SuperAdmin';
}

export function isSuperAdminRole(role?: UserRole | null): boolean {
  return role === 'SuperAdmin';
}
