import { Platform } from 'react-native';

/** Keeps mobile layouts readable when running in a desktop browser. */
export const webPhoneFrame =
  Platform.OS === 'web'
    ? ({ maxWidth: 430, width: '100%', alignSelf: 'center' } as const)
    : ({} as const);
