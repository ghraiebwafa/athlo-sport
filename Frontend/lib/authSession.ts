import { router } from 'expo-router';
import { useAuthStore } from '@/stores/authStore';

let redirecting = false;

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
  return (
    url.includes('/api/auth/login') ||
    url.includes('/api/auth/register') ||
    url.includes('/api/auth/refresh') ||
    url.includes('/api/auth/forgot-password') ||
    url.includes('/api/auth/reset-password')
  );
}
