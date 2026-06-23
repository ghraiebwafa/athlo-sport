import { router } from 'expo-router';
import { useAuthStore } from '@/stores/authStore';

let redirecting = false;

const AUTH_PATHS = [
  '/api/auth/login',
  '/api/auth/register',
  '/api/auth/refresh',
  '/api/auth/logout',
  '/api/auth/forgot-password',
  '/api/auth/reset-password',
] as const;

/** Clears tokens and sends the user to login after session expiry. */
export async function handleUnauthorized() {
  if (redirecting) return;
  redirecting = true;
  try {
    await useAuthStore.getState().clearSession();
    router.replace('/(auth)/login');
  } finally {
    redirecting = false;
  }
}

export function isAuthEndpoint(url?: string): boolean {
  if (!url) return false;
  try {
    const path = new URL(url, 'http://athlo.local').pathname;
    return AUTH_PATHS.some((authPath) => path === authPath || path.endsWith(authPath));
  } catch {
    return AUTH_PATHS.some((authPath) => url.includes(authPath));
  }
}
