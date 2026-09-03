import { Platform } from 'react-native';
import Constants from 'expo-constants';
import * as SecureStore from 'expo-secure-store';
import { managementApi } from '@/lib/api/client';
import { captureException } from '@/lib/telemetry';

const TOKEN_KEY = 'athlo_expo_push_token';

export async function registerPushToken(token: string, platform: string) {
  await managementApi.post('/api/devices/push-token', { token, platform });
}

export async function unregisterPushToken(token: string, platform: string) {
  await managementApi.delete('/api/devices/push-token', {
    data: { token, platform },
  });
}

export async function getStoredPushToken(): Promise<string | null> {
  try {
    return await SecureStore.getItemAsync(TOKEN_KEY);
  } catch {
    return null;
  }
}

/** Best-effort unregister of the stored Expo push token (e.g. on logout). */
export async function unregisterStoredPushToken(): Promise<void> {
  const token = await getStoredPushToken();
  if (!token) return;
  try {
    await unregisterPushToken(token, Platform.OS);
  } catch {
    // session may already be invalid
  }
  try {
    await SecureStore.deleteItemAsync(TOKEN_KEY);
  } catch {
    // ignore
  }
}

/**
 * Requests notification permission and registers an Expo push token with the API.
 * Safe on web / simulators: returns null when push is unavailable.
 */
export async function enablePushNotifications(): Promise<string | null> {
  if (Platform.OS === 'web') {
    return null;
  }

  try {
    const Notifications = await import('expo-notifications');
    const Device = await import('expo-device');

    if (!Device.isDevice) {
      return null;
    }

    const { status: existing } = await Notifications.getPermissionsAsync();
    let finalStatus = existing;
    if (existing !== 'granted') {
      const { status } = await Notifications.requestPermissionsAsync();
      finalStatus = status;
    }
    if (finalStatus !== 'granted') {
      return null;
    }

    const projectId =
      Constants.easConfig?.projectId ??
      Constants.expoConfig?.extra?.eas?.projectId;

    const tokenResponse = projectId
      ? await Notifications.getExpoPushTokenAsync({ projectId })
      : await Notifications.getExpoPushTokenAsync();

    const token = tokenResponse.data;
    await registerPushToken(token, Platform.OS);
    await SecureStore.setItemAsync(TOKEN_KEY, token);
    return token;
  } catch (err) {
    captureException(err, { action: 'enablePushNotifications' });
    return null;
  }
}
