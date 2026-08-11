import { useMutation } from '@tanstack/react-query';
import { router } from 'expo-router';
import { Lock } from 'lucide-react-native';
import { useState } from 'react';
import { Alert, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, View } from 'react-native';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { Button } from '@/components/ui/Button';
import { FormErrorBanner } from '@/components/ui/FormErrorBanner';
import { Input } from '@/components/ui/Input';
import { theme } from '@/constants/theme';
import { changePassword } from '@/lib/api/auth';
import { parseApiError } from '@/lib/api/client';
import { validatePassword } from '@/lib/validatePassword';

export default function ChangePasswordScreen() {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const mutation = useMutation({
    mutationFn: () =>
      changePassword({ currentPassword, newPassword, confirmNewPassword }),
    onSuccess: () => {
      Alert.alert('Password updated', 'Use your new password next time you sign in.', [
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

    if (!currentPassword) {
      setError('Enter your current password.');
      setFieldErrors({ currentPassword: 'Required.' });
      return;
    }
    const next = validatePassword(newPassword);
    if (!next.ok) {
      setError(next.message);
      setFieldErrors({ newPassword: next.message });
      return;
    }
    if (newPassword !== confirmNewPassword) {
      setError('Passwords do not match.');
      setFieldErrors({ confirmNewPassword: 'Passwords do not match.' });
      return;
    }
    if (newPassword === currentPassword) {
      setError('New password must differ from current password.');
      setFieldErrors({ newPassword: 'Choose a different password.' });
      return;
    }
    mutation.mutate();
  };

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Change Password" onBack={() => router.back()} />
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={styles.flex}>
        <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
          <Input
            label="Current Password"
            value={currentPassword}
            onChangeText={setCurrentPassword}
            secureTextEntry
            secureToggle
            autoComplete="password"
            icon={Lock}
            error={fieldErrors.currentPassword}
          />
          <Input
            label="New Password"
            value={newPassword}
            onChangeText={setNewPassword}
            secureTextEntry
            secureToggle
            autoComplete="new-password"
            icon={Lock}
            error={fieldErrors.newPassword}
          />
          <Input
            label="Confirm New Password"
            value={confirmNewPassword}
            onChangeText={setConfirmNewPassword}
            secureTextEntry
            secureToggle
            autoComplete="new-password"
            icon={Lock}
            error={fieldErrors.confirmNewPassword}
          />
          <FormErrorBanner message={error} />
          <Button title="Update Password" onPress={handleSave} loading={mutation.isPending} />
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
