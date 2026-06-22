import { Link, router } from 'expo-router';
import { Lock, Mail } from 'lucide-react-native';
import { useState } from 'react';
import { KeyboardAvoidingView, Platform, Pressable, StyleSheet, Text, View } from 'react-native';
import { AuthHeader } from '@/components/auth/AuthHeader';
import { SocialLogin } from '@/components/auth/SocialLogin';
import { BackgroundScreen } from '@/components/ui/BackgroundScreen';
import { Button } from '@/components/ui/Button';
import { FormErrorBanner } from '@/components/ui/FormErrorBanner';
import { Input } from '@/components/ui/Input';
import { images } from '@/constants/images';
import { theme } from '@/constants/theme';
import { login } from '@/lib/api/auth';
import { parseApiError } from '@/lib/api/client';
import { useAuthStore } from '@/stores/authStore';

export default function LoginScreen() {
  const setSession = useAuthStore((s) => s.setSession);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);

  const handleLogin = async () => {
    setError('');
    setFieldErrors({});

    if (!email.trim()) {
      setError('Please enter your email address.');
      setFieldErrors({ email: 'Email is required.' });
      return;
    }
    if (!password) {
      setError('Please enter your password.');
      setFieldErrors({ password: 'Password is required.' });
      return;
    }

    setLoading(true);
    try {
      const data = await login({ email: email.trim(), password });
      await setSession({
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
        expiresAt: data.expiresAt,
        user: data.user,
      });
      router.replace('/(tabs)');
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
        <AuthHeader showBack={false} />

        <Text style={styles.welcome}>Welcome Back</Text>
        <Text style={styles.subtitle}>Sign in to continue your fitness journey.</Text>

        <View style={styles.form}>
          <Input
            label="Email Address"
            value={email}
            onChangeText={(v) => {
              setEmail(v);
              if (fieldErrors.email) setFieldErrors((e) => ({ ...e, email: '' }));
            }}
            autoCapitalize="none"
            autoComplete="email"
            keyboardType="email-address"
            icon={Mail}
            error={fieldErrors.email}
          />
          <View>
            <Input
              label="Password"
              value={password}
              onChangeText={(v) => {
                setPassword(v);
                if (fieldErrors.password) setFieldErrors((e) => ({ ...e, password: '' }));
              }}
              secureTextEntry
              secureToggle
              autoComplete="password"
              icon={Lock}
              error={fieldErrors.password}
            />
            <Link href="/(auth)/forgot-password" asChild>
              <Pressable style={styles.forgot}>
                <Text style={styles.forgotText}>Forgot Password?</Text>
              </Pressable>
            </Link>
          </View>
          <FormErrorBanner message={error} />
          <Button title="Sign In" onPress={handleLogin} loading={loading} />
          <SocialLogin />
          <Text style={styles.footer}>
            Don&apos;t have an account?{' '}
            <Link href="/(auth)/register" style={styles.footerLink}>Create Account</Link>
          </Text>
        </View>
      </KeyboardAvoidingView>
    </BackgroundScreen>
  );
}

const styles = StyleSheet.create({
  content: { flexGrow: 1, padding: theme.spacing.lg, paddingTop: 8, paddingBottom: 40 },
  keyboard: { flex: 1 },
  welcome: { ...theme.typography.title, color: theme.colors.text, marginBottom: 6, textAlign: 'center' },
  subtitle: { color: theme.colors.textMuted, marginBottom: theme.spacing.lg, lineHeight: 22, textAlign: 'center' },
  form: { gap: theme.spacing.md },
  forgot: { alignSelf: 'flex-end', marginTop: 8 },
  forgotText: { color: theme.colors.primary, fontSize: 14, fontWeight: '500' },
  footer: { color: theme.colors.textMuted, textAlign: 'center', marginTop: 4 },
  footerLink: { color: theme.colors.primary, fontWeight: '600' },
});
