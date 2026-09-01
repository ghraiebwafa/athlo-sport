import { getItem, removeItem, setItem } from '@/lib/storage';
import { refreshSessionIfNeeded } from '@/lib/authRefresh';
import { parseStoredSession } from '@/lib/parseStoredSession';
import { create } from 'zustand';
import type { UserProfile } from '@/lib/types';

const STORAGE_KEY = 'athlo_auth';

async function syncPreferencesFromServer() {
  const { usePreferencesStore } = await import('@/stores/preferencesStore');
  await usePreferencesStore.getState().syncFromServer();
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserProfile;
}

interface AuthState {
  user: UserProfile | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  hydrate: () => Promise<void>;
  setSession: (session: AuthTokens) => Promise<void>;
  clearSession: () => Promise<void>;
}

let cachedTokens: AuthTokens | null = null;

export function getTokens(): AuthTokens | null {
  return cachedTokens;
}

export async function setTokens(session: AuthTokens) {
  cachedTokens = session;
  await setItem(STORAGE_KEY, JSON.stringify(session));
  useAuthStore.setState({
    user: session.user,
    isAuthenticated: true,
    isLoading: false,
  });
  void syncPreferencesFromServer();
}

export async function clearTokens() {
  cachedTokens = null;
  await removeItem(STORAGE_KEY);
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  isLoading: true,

  hydrate: async () => {
    try {
      const raw = await getItem(STORAGE_KEY);
      if (!raw) {
        set({ isLoading: false, isAuthenticated: false, user: null });
        return;
      }

      const session = parseStoredSession(raw);
      if (!session) {
        await removeItem(STORAGE_KEY);
        cachedTokens = null;
        set({ isLoading: false, isAuthenticated: false, user: null });
        return;
      }

      const activeSession = await refreshSessionIfNeeded(session);
      if (!activeSession) {
        set({ isLoading: false, isAuthenticated: false, user: null });
        return;
      }

      cachedTokens = activeSession;
      set({ user: activeSession.user, isAuthenticated: true, isLoading: false });
      void syncPreferencesFromServer();
    } catch {
      await removeItem(STORAGE_KEY);
      cachedTokens = null;
      set({ isLoading: false, isAuthenticated: false, user: null });
    }
  },

  setSession: async (session) => {
    await setTokens(session);
  },

  clearSession: async () => {
    await clearTokens();
    set({ user: null, isAuthenticated: false, isLoading: false });
  },
}));
