import { router } from 'expo-router';
import { useState } from 'react';
import { Alert, KeyboardAvoidingView, Platform, StyleSheet, Text, View } from 'react-native';
import { AuthHeader } from '@/components/auth/AuthHeader';
import { BackgroundScreen } from '@/components/ui/BackgroundScreen';
import { Button } from '@/components/ui/Button';
import { FormErrorBanner } from '@/components/ui/FormErrorBanner';
import { Input } from '@/components/ui/Input';
import { images } from '@/constants/images';
import { theme } from '@/constants/theme';
import { forgotPassword, resetPassword } from '@/lib/api/auth';
import { parseApiError } from '@/lib/api/client';
import { validateEmail, validatePassword } from '@/lib/validatePassword';

export default function ForgotPasswordScreen() {
  const [email, setEmail] = useState('');
  const [token, setToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [step, setStep] = useState<'request' | 'reset'>('request');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const clearField = (field: string) => {
    if (fieldErrors[field]) setFieldErrors((prev) => ({ ...prev, [field]: '' }));
  };

  const handleRequest = async () => {
    setError('');
    setFieldErrors({});
    if (!email.trim()) {
      setError('Please enter your email address.');
      setFieldErrors({ email: 'Email is required.' });
      return;
    }
    const emailResult = validateEmail(email);
    if (!emailResult.ok) {
      setError(emailResult.message);
      setFieldErrors({ email: emailResult.message });
      return;
    }

    setLoading(true);
    try {
      const data = await forgotPassword(email.trim());
      if (__DEV__ && data.resetToken) setToken(data.resetToken);
      Alert.alert('Check your email', data.message);
      setStep('reset');
    } catch (err) {
      const parsed = parseApiError(err);
      setError(parsed.message);
      setFieldErrors(parsed.fieldErrors);
    } finally {
      setLoading(false);
    }
  };

  const handleReset = async () => {
    setError('');
    setFieldErrors({});
    const nextFieldErrors: Record<string, string> = {};

    if (!token.trim()) nextFieldErrors.token = 'Reset token is required.';

    const passwordResult = validatePassword(newPassword);
    if (!passwordResult.ok) nextFieldErrors.newPassword = passwordResult.message;

    if (!confirmPassword) nextFieldErrors.confirmPassword = 'Please confirm your password.';
    else if (confirmPassword !== newPassword) nextFieldErrors.confirmPassword = 'Passwords do not match.';

    if (Object.keys(nextFieldErrors).length > 0) {
      setFieldErrors(nextFieldErrors);
      setError(Object.values(nextFieldErrors)[0]);
      return;
    }

    setLoading(true);
    try {
      await resetPassword(token.trim(), newPassword, confirmPassword);
      Alert.alert('Success', 'Password updated. You can sign in now.');
      router.replace('/(auth)/login');
    } catch (err) {
      const parsed = parseApiError(err);
      setError(parsed.message);
      setFieldErrors(parsed.fieldErrors);
    } finally {
      setLoading(false);
    }
  };

  return (
    <BackgroundScreen source={images.bgAuth} overlayOpacity={0.58} scroll contentStyle={styles.content}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={styles.keyboard}>
        <AuthHeader backFallback="/(auth)/login" />

        <Text style={styles.title}>{step === 'request' ? 'Forgot Password' : 'Reset Password'}</Text>
        {step === 'request' ? (
          <View style={styles.form}>
            <Input
              label="Email"
              value={email}
              onChangeText={(v) => {
                setEmail(v);
                clearField('email');
              }}
              autoCapitalize="none"
              autoComplete="email"
              keyboardType="email-address"
              error={fieldErrors.email}
            />
            <FormErrorBanner message={error} />
            <Button title="Send Reset Link" onPress={handleRequest} loading={loading} />
          </View>
        ) : (
          <View style={styles.form}>
            <Input
              label="Reset token"
              value={token}
              onChangeText={(v) => {
                setToken(v);
                clearField('token');
              }}
              autoCapitalize="none"
              error={fieldErrors.token}
            />
            <Input
              label="New password"
              value={newPassword}
              onChangeText={(v) => {
                setNewPassword(v);
                clearField('newPassword');
              }}
              secureTextEntry
              secureToggle
              autoComplete="new-password"
              error={fieldErrors.newPassword}
            />
            <Input
              label="Confirm password"
              value={confirmPassword}
              onChangeText={(v) => {
                setConfirmPassword(v);
                clearField('confirmPassword');
              }}
              secureTextEntry
              secureToggle
              autoComplete="new-password"
              error={fieldErrors.confirmPassword}
            />
            <FormErrorBanner message={error} />
            <Button title="Reset Password" onPress={handleReset} loading={loading} />
          </View>
        )}
      </KeyboardAvoidingView>
    </BackgroundScreen>
  );
}

const styles = StyleSheet.create({
  content: {
    flexGrow: 1,
    padding: theme.spacing.lg,
    paddingTop: 8,
    paddingBottom: 40,
  },
  keyboard: { flex: 1 },
  title: {
    fontSize: 28,
    fontWeight: '700',
    color: theme.colors.text,
    marginBottom: 24,
    textAlign: 'center',
  },
  form: { gap: theme.spacing.md },
});
