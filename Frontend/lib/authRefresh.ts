import axios from 'axios';
import { config } from '@/lib/config';
import type { AuthResponse } from '@/lib/types';
import type { AuthTokens } from '@/stores/authStore';
import { clearTokens, setTokens } from '@/stores/authStore';

const REFRESH_BUFFER_MS = 60_000;

/** Refresh tokens via the auth API and persist the new session. */
export async function performTokenRefresh(refreshToken: string): Promise<AuthTokens | null> {
  try {
    const { data } = await axios.post<AuthResponse>(`${config.authApiUrl}/api/auth/refresh`, {
      refreshToken,
    });
    const refreshed: AuthTokens = {
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      expiresAt: data.expiresAt,
      user: data.user,
    };
    await setTokens(refreshed);
    return refreshed;
  } catch {
    await clearTokens();
    return null;
  }
}

export async function refreshSessionIfNeeded(session: AuthTokens): Promise<AuthTokens | null> {
  if (!session.refreshToken) {
    await clearTokens();
    return null;
  }

  const expiresAt = new Date(session.expiresAt);
  if (Number.isNaN(expiresAt.getTime())) {
    await clearTokens();
    return null;
  }

  if (expiresAt.getTime() - Date.now() > REFRESH_BUFFER_MS) {
    return session;
  }

  return performTokenRefresh(session.refreshToken);
}
