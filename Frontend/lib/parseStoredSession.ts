import type { AuthTokens } from '@/stores/authStore';
import type { UserProfile } from '@/lib/types';

function isUserProfile(value: unknown): value is UserProfile {
  if (!value || typeof value !== 'object') return false;
  const user = value as UserProfile;
  return (
    typeof user.id === 'string' &&
    typeof user.email === 'string' &&
    typeof user.fullName === 'string' &&
    typeof user.role === 'string'
  );
}

/** Runtime validation for persisted auth session JSON. */
export function parseStoredSession(raw: string): AuthTokens | null {
  try {
    const parsed: unknown = JSON.parse(raw);
    if (!parsed || typeof parsed !== 'object') return null;

    const session = parsed as AuthTokens;
    if (
      typeof session.accessToken !== 'string' ||
      typeof session.refreshToken !== 'string' ||
      typeof session.expiresAt !== 'string' ||
      !isUserProfile(session.user)
    ) {
      return null;
    }

    const expiresAt = new Date(session.expiresAt);
    if (Number.isNaN(expiresAt.getTime())) return null;

    return session;
  } catch {
    return null;
  }
}
