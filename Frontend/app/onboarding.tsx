import { Redirect, router } from 'expo-router';
import { BarChart3, Dumbbell, Trophy } from 'lucide-react-native';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { AthloLogo } from '@/components/brand/AthloLogo';
import { FeatureCard } from '@/components/onboarding/FeatureCard';
import { Button } from '@/components/ui/Button';
import { Screen } from '@/components/ui/Screen';
import { theme } from '@/constants/theme';
import { webPhoneFrame } from '@/lib/layout';
import { setOnboardingComplete } from '@/lib/onboarding';
import { ROUTES } from '@/lib/routes';
import { useAuthStore } from '@/stores/authStore';

async function finishOnboarding(path: '/(auth)/register' | '/(auth)/login') {
  await setOnboardingComplete();
  router.replace(path);
}

function OnboardingContent() {
  return (
    <Screen scroll contentStyle={styles.content}>
      <Pressable style={styles.skip} onPress={() => finishOnboarding('/(auth)/login')}>
        <Text style={styles.skipText}>Skip</Text>
      </Pressable>

      <AthloLogo size="xl" layout="horizontal" showTagline={false} />

      <Text style={styles.hero}>
        Your Journey{'\n'}
        <Text style={styles.heroAccent}>Starts Here</Text>
      </Text>
      <Text style={styles.lead}>Smart training. Real progress. A stronger you.</Text>

      <View style={styles.features}>
        <FeatureCard
          icon={BarChart3}
          title="Track Progress"
          description="Monitor your workouts, stats and personal bests."
        />
        <FeatureCard
          icon={Dumbbell}
          title="Personalized Plans"
          description="Workouts tailored to your goals and fitness level."
        />
        <FeatureCard
          icon={Trophy}
          title="Stay Motivated"
          description="Build healthy habits and achieve your goals with consistency."
        />
      </View>

      <View style={styles.dots}>
        <View style={[styles.dot, styles.dotActive]} />
      </View>

      <Button title="Get Started" onPress={() => finishOnboarding('/(auth)/register')} />
      <Pressable onPress={() => finishOnboarding('/(auth)/login')}>
        <Text style={styles.footer}>
          Already have an account? <Text style={styles.footerLink}>Log In</Text>
        </Text>
      </Pressable>
    </Screen>
  );
}

export default function OnboardingScreen() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const isLoading = useAuthStore((s) => s.isLoading);

  if (isLoading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator color={theme.colors.primary} size="large" />
      </View>
    );
  }

  if (isAuthenticated) {
    return <Redirect href={ROUTES.home} />;
  }

  return <OnboardingContent />;
}

const styles = StyleSheet.create({
  centered: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: theme.colors.background,
  },
  content: { paddingTop: theme.spacing.md, paddingBottom: theme.spacing.xl, ...webPhoneFrame },
  skip: { alignSelf: 'flex-end', marginBottom: theme.spacing.md },
  skipText: { color: theme.colors.primary, fontWeight: '600', fontSize: 16 },
  hero: {
    ...theme.typography.hero,
    color: theme.colors.text,
    textAlign: 'center',
    marginTop: theme.spacing.lg,
  },
  heroAccent: { color: theme.colors.primary },
  lead: {
    color: theme.colors.textMuted,
    textAlign: 'center',
    marginTop: theme.spacing.sm,
    marginBottom: theme.spacing.lg,
    lineHeight: 22,
  },
  features: { marginBottom: theme.spacing.lg },
  dots: { flexDirection: 'row', justifyContent: 'center', gap: 8, marginBottom: theme.spacing.lg },
  dot: { width: 8, height: 8, borderRadius: 4, backgroundColor: theme.colors.surfaceLight },
  dotActive: { backgroundColor: theme.colors.primary, width: 20 },
  footer: { color: theme.colors.textMuted, textAlign: 'center', marginTop: theme.spacing.md },
  footerLink: { color: theme.colors.primary, fontWeight: '600' },
});
