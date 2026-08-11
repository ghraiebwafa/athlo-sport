import { Stack } from 'expo-router';
import { AuthGuard } from '@/components/auth/AuthGuard';

export default function ProtectedLayout() {
  return (
    <AuthGuard>
      <Stack screenOptions={{ headerShown: false }}>
        <Stack.Screen name="(tabs)" />
        <Stack.Screen name="program/[id]" />
        <Stack.Screen name="programs/saved" />
        <Stack.Screen name="workout/active" />
        <Stack.Screen name="workout/complete" />
        <Stack.Screen name="workout/history" />
        <Stack.Screen name="profile/edit" />
        <Stack.Screen name="profile/change-password" />
        <Stack.Screen name="profile/edit-goals" />
        <Stack.Screen name="admin/index" />
        <Stack.Screen name="admin/exercises" />
        <Stack.Screen name="admin/categories" />
        <Stack.Screen name="admin/programs" />
        <Stack.Screen name="admin/users" />
      </Stack>
    </AuthGuard>
  );
}
