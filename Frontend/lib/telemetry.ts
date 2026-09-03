import { config } from '@/lib/config';

type TelemetryPayload = {
  message: string;
  stack?: string;
  extra?: Record<string, unknown>;
};

/**
 * Lightweight crash/event reporter.
 * When EXPO_PUBLIC_SENTRY_DSN is set in a native/EAS build, wire @sentry/react-native here.
 * Until then, logs in __DEV__ and no-ops in production.
 */
export function captureException(error: unknown, extra?: Record<string, unknown>) {
  const payload: TelemetryPayload = {
    message: error instanceof Error ? error.message : String(error),
    stack: error instanceof Error ? error.stack : undefined,
    extra,
  };

  if (typeof __DEV__ !== 'undefined' && __DEV__) {
    console.warn('[telemetry]', payload);
  }

  // Placeholder for future Sentry.init({ dsn: process.env.EXPO_PUBLIC_SENTRY_DSN })
  void config;
}

export function captureMessage(message: string, extra?: Record<string, unknown>) {
  if (typeof __DEV__ !== 'undefined' && __DEV__) {
    console.info('[telemetry]', message, extra);
  }
}
