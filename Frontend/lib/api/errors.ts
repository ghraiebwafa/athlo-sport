import axios from 'axios';
import { config } from '@/lib/config';
import type { ApiErrorResponse } from '@/lib/types';

export interface ParsedApiError {
  message: string;
  fieldErrors: Record<string, string>;
  code?: string;
}

function fieldErrorsFromDetails(details?: { field: string; message: string }[]) {
  const fieldErrors: Record<string, string> = {};
  if (!details?.length) return fieldErrors;

  for (const detail of details) {
    if (detail.field) fieldErrors[detail.field] = detail.message;
  }
  return fieldErrors;
}

function isDevEnvironment(): boolean {
  if (typeof __DEV__ !== 'undefined') return __DEV__;
  return process.env.NODE_ENV !== 'production';
}

function networkMessage(): string {
  if (isDevEnvironment()) {
    return `Cannot reach the server. Start the backend (\`docker compose up -d\`) and confirm the API is up at ${config.authApiUrl}.`;
  }
  return 'Cannot reach the server. Check your connection and try again.';
}

function statusFallback(status: number): string {
  switch (status) {
    case 400:
      return 'Invalid request. Please check your input.';
    case 401:
      return 'Invalid email or password.';
    case 403:
      return 'You do not have permission to do that.';
    case 404:
      return 'The requested resource was not found.';
    case 409:
      return 'This action conflicts with existing data.';
    case 429:
      return 'Too many attempts. Please wait a minute and try again.';
    case 500:
    case 502:
    case 503:
      return 'Server error. Please try again in a moment.';
    default:
      return 'Something went wrong. Please try again.';
  }
}

export function parseApiError(error: unknown): ParsedApiError {
  const fallback: ParsedApiError = {
    message: 'Something went wrong. Please try again.',
    fieldErrors: {},
  };

  if (!axios.isAxiosError(error)) {
    if (error instanceof Error && error.message) {
      return { ...fallback, message: error.message };
    }
    return fallback;
  }

  if (!error.response) {
    if (error.code === 'ECONNABORTED') {
      return { message: 'Request timed out. Please try again.', fieldErrors: {}, code: 'TIMEOUT' };
    }
    if (error.code === 'ERR_NETWORK' || error.message === 'Network Error') {
      return { message: networkMessage(), fieldErrors: {}, code: 'NETWORK' };
    }
    return { message: networkMessage(), fieldErrors: {}, code: 'NETWORK' };
  }

  const data = error.response.data as ApiErrorResponse | undefined;
  const apiError = data?.api?.error;
  const fieldErrors = fieldErrorsFromDetails(apiError?.details);
  const status = error.response.status;

  let message =
    apiError?.message ||
    (status === 401 ? 'Invalid email or password.' : statusFallback(status));

  if (apiError?.code === 'VALIDATION_FAILED' && apiError.details?.length) {
    const detailSummary = apiError.details.map((d) => d.message).join(' ');
    message = detailSummary || apiError.message || 'Please fix the highlighted fields.';
  }

  return {
    message,
    fieldErrors,
    code: apiError?.code,
  };
}

export function getApiErrorMessage(error: unknown): string {
  return parseApiError(error).message;
}
