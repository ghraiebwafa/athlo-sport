import axios from 'axios';
import { config } from '@/lib/config';
import type { AuthResponse } from '@/lib/types';
import type { AuthTokens } from '@/stores/authStore';
import { clearTokens, setTokens } from '@/stores/authStore';

const REFRESH_BUFFER_MS = 60_000;

export async function refreshSessionIfNeeded(session: AuthTokens): Promise<AuthTokens | null> {
  if (!session.refreshToken) return null;

  const expiresAt = new Date(session.expiresAt);
  if (Number.isNaN(expiresAt.getTime())) return null;

  if (expiresAt.getTime() - Date.now() > REFRESH_BUFFER_MS) {
    return session;
  }

  try {
    const { data } = await axios.post<AuthResponse>(`${config.authApiUrl}/api/auth/refresh`, {
      refreshToken: session.refreshToken,
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
