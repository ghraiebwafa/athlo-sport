import { StyleSheet, Text, View } from 'react-native';
import { Button } from '@/components/ui/Button';
import { theme } from '@/constants/theme';

interface QueryStateProps {
  title?: string;
  message: string;
  onRetry?: () => void;
  retryLabel?: string;
  variant?: 'error' | 'empty';
}

/** Centered error / empty state with optional retry. */
export function QueryState({
  title,
  message,
  onRetry,
  retryLabel = 'Retry',
  variant = 'empty',
}: QueryStateProps) {
  const resolvedTitle =
    title ?? (variant === 'error' ? 'Something went wrong' : undefined);

  return (
    <View
      style={styles.wrap}
      accessibilityRole={variant === 'error' ? 'alert' : 'summary'}
    >
      {resolvedTitle ? <Text style={styles.title}>{resolvedTitle}</Text> : null}
      <Text style={styles.message}>{message}</Text>
      {onRetry ? <Button title={retryLabel} onPress={onRetry} variant="secondary" /> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: theme.colors.background,
    padding: theme.spacing.lg,
    gap: theme.spacing.md,
  },
  title: {
    color: theme.colors.text,
    fontWeight: '700',
    fontSize: 17,
    textAlign: 'center',
  },
  message: {
    color: theme.colors.textMuted,
    textAlign: 'center',
    fontSize: 15,
    lineHeight: 22,
  },
});
