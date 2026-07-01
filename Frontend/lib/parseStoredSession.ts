import type { AuthTokens } from '@/stores/authStore';
import type { FitnessGoal, UserProfile, UserRole } from '@/lib/types';

const FITNESS_GOALS: FitnessGoal[] = ['LoseWeight', 'BuildMuscle', 'StayActive'];
const USER_ROLES: UserRole[] = ['User', 'Admin', 'SuperAdmin'];

function isFitnessGoal(value: unknown): value is FitnessGoal {
  return typeof value === 'string' && FITNESS_GOALS.includes(value as FitnessGoal);
}

function isUserRole(value: unknown): value is UserRole {
  return typeof value === 'string' && USER_ROLES.includes(value as UserRole);
}

function isUserProfile(value: unknown): value is UserProfile {
  if (!value || typeof value !== 'object') return false;
  const user = value as UserProfile;
  return (
    typeof user.id === 'string' &&
    typeof user.email === 'string' &&
    typeof user.fullName === 'string' &&
    typeof user.currentWeight === 'number' &&
    Number.isFinite(user.currentWeight) &&
    typeof user.goalWeight === 'number' &&
    Number.isFinite(user.goalWeight) &&
    isFitnessGoal(user.fitnessGoal) &&
    isUserRole(user.role) &&
    typeof user.goalProgressPercent === 'number' &&
    Number.isFinite(user.goalProgressPercent)
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
