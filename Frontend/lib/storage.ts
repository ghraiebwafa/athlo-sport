import { Platform } from 'react-native';
import * as SecureStore from 'expo-secure-store';

const WEB_STORAGE = typeof sessionStorage !== 'undefined' ? sessionStorage : null;

/** SecureStore on native; sessionStorage on web (cleared when the tab closes). */
export async function getItem(key: string): Promise<string | null> {
  if (Platform.OS === 'web') {
    return WEB_STORAGE?.getItem(key) ?? null;
  }
  return SecureStore.getItemAsync(key);
}

export async function setItem(key: string, value: string): Promise<void> {
  if (Platform.OS === 'web') {
    WEB_STORAGE?.setItem(key, value);
    return;
  }
  await SecureStore.setItemAsync(key, value);
}

export async function removeItem(key: string): Promise<void> {
  if (Platform.OS === 'web') {
    WEB_STORAGE?.removeItem(key);
    return;
  }
  await SecureStore.deleteItemAsync(key);
}
