import { Link, router } from 'expo-router';
import { Lock, Mail, User } from 'lucide-react-native';
import { useState } from 'react';
import { KeyboardAvoidingView, Platform, StyleSheet, Text, View } from 'react-native';
import { AuthHeader } from '@/components/auth/AuthHeader';
import { GoalSelector } from '@/components/auth/GoalSelector';
import { BackgroundScreen } from '@/components/ui/BackgroundScreen';
import { Button } from '@/components/ui/Button';
import { FormErrorBanner } from '@/components/ui/FormErrorBanner';
import { Input } from '@/components/ui/Input';
import { images } from '@/constants/images';
import { theme } from '@/constants/theme';
import { register } from '@/lib/api/auth';
import { parseApiError } from '@/lib/api/client';
import { validateRegisterForm } from '@/lib/validateRegister';
import { ROUTES } from '@/lib/routes';
import { ROUTES } from '@/lib/routes';
import { useAuthStore } from '@/stores/authStore';

export default function RegisterScreen() {
  const setSession = useAuthStore((s) => s.setSession);
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [fitnessGoal, setFitnessGoal] = useState<FitnessGoal>('LoseWeight');
  const [currentWeight, setCurrentWeight] = useState('70');
  const [goalWeight, setGoalWeight] = useState('65');
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);

  const clearField = (field: string) => {
    if (fieldErrors[field]) setFieldErrors((prev) => ({ ...prev, [field]: '' }));
  };

  const handleRegister = async () => {
    setError('');
    setFieldErrors({});

    const validation = validateRegisterForm({
      fullName,
      email,
      password,
      confirmPassword,
      currentWeight,
      goalWeight,
    });
    if (!validation.ok) {
      setError(validation.message || 'Please fix the highlighted fields.');
      setFieldErrors(validation.fieldErrors);
      return;
    }

    const weight = Number.parseFloat(currentWeight);
    const target = Number.parseFloat(goalWeight);

    setLoading(true);
    try {
      const data = await register({
        fullName: fullName.trim(),
        email: email.trim(),
        password,
        confirmPassword,
        fitnessGoal,
        currentWeight: weight,
        goalWeight: target,
      });
      await setSession({
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
        expiresAt: data.expiresAt,
        user: data.user,
      });
      router.replace(ROUTES.home);
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
        <AuthHeader backFallback={ROUTES.login} />

        <Text style={styles.title}>Create Account</Text>
        <Text style={styles.subtitle}>Let&apos;s build your fitness journey together.</Text>

        <View style={styles.form}>
          <Input
            label="Full Name"
            value={fullName}
            onChangeText={(v) => {
              setFullName(v);
              clearField('fullName');
            }}
            icon={User}
            error={fieldErrors.fullName}
          />
          <Input
            label="Email Address"
            value={email}
            onChangeText={(v) => {
              setEmail(v);
              clearField('email');
            }}
            autoCapitalize="none"
            autoComplete="email"
            keyboardType="email-address"
            icon={Mail}
            error={fieldErrors.email}
          />
          <Input
            label="Password"
            value={password}
            onChangeText={(v) => {
              setPassword(v);
              clearField('password');
            }}
            secureTextEntry
            secureToggle
            autoComplete="new-password"
            icon={Lock}
            error={fieldErrors.password}
          />
          <Input
            label="Confirm Password"
            value={confirmPassword}
            onChangeText={(v) => {
              setConfirmPassword(v);
              clearField('confirmPassword');
            }}
            secureTextEntry
            secureToggle
            autoComplete="new-password"
            icon={Lock}
            error={fieldErrors.confirmPassword}
          />
          <GoalSelector value={fitnessGoal} onChange={setFitnessGoal} />
          <Input
            label="Current Weight (kg)"
            value={currentWeight}
            onChangeText={(v) => {
              setCurrentWeight(v);
              clearField('currentWeight');
            }}
            keyboardType="decimal-pad"
            error={fieldErrors.currentWeight}
          />
          <Input
            label="Goal Weight (kg)"
            value={goalWeight}
            onChangeText={(v) => {
              setGoalWeight(v);
              clearField('goalWeight');
            }}
            keyboardType="decimal-pad"
            error={fieldErrors.goalWeight}
          />
          <FormErrorBanner message={error} />
          <Button title="Create Account" onPress={handleRegister} loading={loading} />
          <Text style={styles.footer}>
            Already have an account?{' '}
            <Link href={ROUTES.login} style={styles.footerLink}>Sign In</Link>
          </Text>
        </View>
      </KeyboardAvoidingView>
    </BackgroundScreen>
  );
}

const styles = StyleSheet.create({
  content: { flexGrow: 1, padding: theme.spacing.lg, paddingTop: 8, paddingBottom: 40 },
  keyboard: { flex: 1 },
  title: { ...theme.typography.title, color: theme.colors.text, marginBottom: 6, textAlign: 'center' },
  subtitle: { color: theme.colors.textMuted, marginBottom: theme.spacing.lg, lineHeight: 22, textAlign: 'center' },
  form: { gap: theme.spacing.md },
  footer: { color: theme.colors.textMuted, textAlign: 'center' },
  footerLink: { color: theme.colors.primary, fontWeight: '600' },
});
