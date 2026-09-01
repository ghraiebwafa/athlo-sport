import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { handleUnauthorized, isAuthEndpoint } from '@/lib/authSession';
import { refreshAccessTokenLocked } from '@/lib/authRefresh';
import { config } from '@/lib/config';
import { getTokens } from '@/stores/authStore';

export { getApiErrorMessage, parseApiError, isNetworkError, isNotFoundError } from '@/lib/api/errors';
export type { ParsedApiError } from '@/lib/api/errors';

export const authApi = axios.create({
  baseURL: config.authApiUrl,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30_000,
});

export const managementApi = axios.create({
  baseURL: config.managementApiUrl,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30_000,
});

type RetryableRequestConfig = InternalAxiosRequestConfig & { _retry?: boolean };

async function attachToken(cfg: InternalAxiosRequestConfig) {
  const tokens = getTokens();
  if (tokens?.accessToken) {
    cfg.headers.Authorization = `Bearer ${tokens.accessToken}`;
  }
  return cfg;
}

authApi.interceptors.request.use(attachToken);
managementApi.interceptors.request.use(attachToken);

let isRefreshing = false;
let refreshQueue: ((token: string | null) => void)[] = [];

function processQueue(token: string | null) {
  refreshQueue.forEach((cb) => cb(token));
  refreshQueue = [];
}

async function refreshAccessToken(): Promise<string | null> {
  const tokens = getTokens();
  if (!tokens?.refreshToken) return null;
  const refreshed = await refreshAccessTokenLocked(tokens.refreshToken);
  return refreshed?.accessToken ?? null;
}

function setupRefreshInterceptor(instance: typeof authApi) {
  instance.interceptors.response.use(
    (res) => res,
    async (error: AxiosError) => {
      const original = error.config as RetryableRequestConfig | undefined;
      if (!original || error.response?.status !== 401 || isAuthEndpoint(original.url)) {
        return Promise.reject(error);
      }

      if (original._retry) {
        await handleUnauthorized();
        return Promise.reject(error);
      }

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          refreshQueue.push((token) => {
            if (!token) {
              void handleUnauthorized();
              return reject(error);
            }
            original.headers.Authorization = `Bearer ${token}`;
            resolve(instance(original));
          });
        });
      }

      isRefreshing = true;
      const newToken = await refreshAccessToken();
      isRefreshing = false;
      processQueue(newToken);

      if (!newToken) {
        await handleUnauthorized();
        return Promise.reject(error);
      }
      original._retry = true;
      original.headers.Authorization = `Bearer ${newToken}`;
      return instance(original);
    }
  );
}

setupRefreshInterceptor(authApi);
setupRefreshInterceptor(managementApi);
