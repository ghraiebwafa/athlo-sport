import { StyleSheet, Text, View } from 'react-native';
import { Button } from '@/components/ui/Button';
import { theme } from '@/constants/theme';

interface QueryStateProps {
  message: string;
  onRetry?: () => void;
  retryLabel?: string;
}

/** Centered error / empty state with optional retry. */
export function QueryState({ message, onRetry, retryLabel = 'Retry' }: QueryStateProps) {
  return (
    <View style={styles.wrap} accessibilityRole="alert">
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
  message: {
    color: theme.colors.textMuted,
    textAlign: 'center',
    fontSize: 15,
    lineHeight: 22,
  },
});
