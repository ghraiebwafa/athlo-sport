const authApiUrl = process.env.EXPO_PUBLIC_AUTH_API_URL ?? 'http://localhost:5001';
const managementApiUrl = process.env.EXPO_PUBLIC_MANAGEMENT_API_URL ?? 'http://localhost:5000';

export const config = {
  authApiUrl,
  managementApiUrl,
} as const;
