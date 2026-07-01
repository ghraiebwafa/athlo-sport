import { router } from 'expo-router';
import { clearAppQueryCache } from '@/lib/queryClient';
import { ROUTES } from '@/lib/routes';
import { useAuthStore } from '@/stores/authStore';

let redirecting = false;

/** Clears server cache, session, and sends the user to login. */
export async function signOutAndRedirect() {
  await clearAppQueryCache();
  await useAuthStore.getState().clearSession();
  router.replace(ROUTES.login);
}

/** Clears tokens and sends the user to login after session expiry. */
export async function handleUnauthorized() {
  if (redirecting) return;
  redirecting = true;
  try {
    await signOutAndRedirect();
  } finally {
    redirecting = false;
  }
}

const AUTH_PATHS = [
  '/api/auth/login',
  '/api/auth/register',
  '/api/auth/refresh',
  '/api/auth/logout',
  '/api/auth/forgot-password',
  '/api/auth/reset-password',
] as const;

export function isAuthEndpoint(url?: string): boolean {
  if (!url) return false;
  try {
    const path = new URL(url, 'http://athlo.local').pathname;
    return AUTH_PATHS.some((authPath) => path === authPath || path.endsWith(authPath));
  } catch {
    return AUTH_PATHS.some((authPath) => url.includes(authPath));
  }
}
