import { Redirect } from 'expo-router';
import { useEffect, useState } from 'react';
import { SplashView } from '@/components/brand/SplashView';
import { hasCompletedOnboarding } from '@/lib/onboarding';
import { ROUTES } from '@/lib/routes';
import { useAuthStore } from '@/stores/authStore';

export default function Index() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const isLoading = useAuthStore((s) => s.isLoading);
  const [onboardingDone, setOnboardingDone] = useState<boolean | null>(null);

  useEffect(() => {
    hasCompletedOnboarding().then(setOnboardingDone);
  }, []);

  if (isLoading || onboardingDone === null) {
    return <SplashView />;
  }

  if (!onboardingDone) {
    return <Redirect href="/onboarding" />;
  }

  return isAuthenticated ? <Redirect href={ROUTES.home} /> : <Redirect href={ROUTES.login} />;
}
