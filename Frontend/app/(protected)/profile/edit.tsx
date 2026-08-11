import { useMutation, useQueryClient } from '@tanstack/react-query';
import { router } from 'expo-router';
import { useState } from 'react';
import { Alert, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, View } from 'react-native';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { Button } from '@/components/ui/Button';
import { FormErrorBanner } from '@/components/ui/FormErrorBanner';
import { Input } from '@/components/ui/Input';
import { theme } from '@/constants/theme';
import { updateProfile } from '@/lib/api/auth';
import { parseApiError } from '@/lib/api/client';
import { ROUTES } from '@/lib/routes';
import { getTokens, setTokens, useAuthStore } from '@/stores/authStore';

export default function EditProfileScreen() {
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();
  const [fullName, setFullName] = useState(user?.fullName ?? '');
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const mutation = useMutation({
    mutationFn: () => updateProfile({ fullName: fullName.trim() }),
    onSuccess: async (profile) => {
      const tokens = getTokens();
      if (tokens) await setTokens({ ...tokens, user: profile });
      await queryClient.invalidateQueries({ queryKey: ['profile'] });
      Alert.alert('Saved', 'Your profile was updated.', [
        { text: 'OK', onPress: () => router.back() },
      ]);
    },
    onError: (err) => {
      const parsed = parseApiError(err);
      setError(parsed.message);
      setFieldErrors(parsed.fieldErrors);
    },
  });

  const handleSave = () => {
    setError('');
    setFieldErrors({});
    if (!fullName.trim()) {
      setError('Please enter your name.');
      setFieldErrors({ fullName: 'Full name is required.' });
      return;
    }
    if (fullName.trim().length > 100) {
      setError('Name is too long.');
      setFieldErrors({ fullName: 'Maximum 100 characters.' });
      return;
    }
    mutation.mutate();
  };

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Edit Profile" onBack={() => router.back()} />
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={styles.flex}>
        <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
          <Input
            label="Full Name"
            value={fullName}
            onChangeText={(v) => {
              setFullName(v);
              if (fieldErrors.fullName) setFieldErrors((e) => ({ ...e, fullName: '' }));
            }}
            autoComplete="name"
            error={fieldErrors.fullName}
          />
          <Input label="Email" value={user?.email ?? ''} editable={false} />
          <FormErrorBanner message={error} />
          <Button title="Save Changes" onPress={handleSave} loading={mutation.isPending} />
          <Button
            title="Change Password"
            variant="secondary"
            onPress={() => router.push(ROUTES.changePassword)}
          />
        </ScrollView>
      </KeyboardAvoidingView>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  flex: { flex: 1 },
  content: { padding: theme.spacing.md, gap: theme.spacing.md, paddingBottom: 40 },
});
