import { router } from 'expo-router';
import { ChevronLeft } from 'lucide-react-native';
import { Pressable, StyleSheet, View } from 'react-native';
import { AthloLogo } from '@/components/brand/AthloLogo';
import { theme } from '@/constants/theme';

interface AuthHeaderProps {
  showBack?: boolean;
  backFallback?: '/onboarding' | '/(auth)/login';
}

export function AuthHeader({ showBack = true, backFallback = '/onboarding' }: AuthHeaderProps) {
  const handleBack = () => {
    if (router.canGoBack()) {
      router.back();
    } else {
      router.replace(backFallback);
    }
  };

  return (
    <View style={styles.wrap}>
      {showBack ? (
        <Pressable style={styles.back} onPress={handleBack} hitSlop={12}>
          <ChevronLeft color={theme.colors.text} size={28} />
        </Pressable>
      ) : null}
      <AthloLogo size="xl" showTagline={false} />
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: {
    alignItems: 'center',
    marginBottom: theme.spacing.xl,
    paddingTop: theme.spacing.md,
    paddingBottom: theme.spacing.sm,
  },
  back: {
    position: 'absolute',
    left: 0,
    top: theme.spacing.sm,
    zIndex: 1,
  },
});
