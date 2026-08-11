import { Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import { formatMmSs } from '@/lib/workoutTimer';

const PRESETS = [60, 90, 120] as const;

interface RestTimerProps {
  remainingSeconds: number;
  paused?: boolean;
  onSkip: () => void;
  onAddSeconds: (seconds: number) => void;
  onSetDuration: (seconds: number) => void;
}

export function RestTimer({
  remainingSeconds,
  paused,
  onSkip,
  onAddSeconds,
  onSetDuration,
}: RestTimerProps) {
  return (
    <View style={styles.card} accessibilityRole="timer" accessibilityLabel={`Rest ${remainingSeconds} seconds`}>
      <Text style={styles.label}>Rest{paused ? ' · Paused' : ''}</Text>
      <Text style={styles.time}>{formatMmSs(remainingSeconds)}</Text>

      <View style={styles.presets}>
        {PRESETS.map((seconds) => (
          <Pressable
            key={seconds}
            style={styles.preset}
            onPress={() => onSetDuration(seconds)}
            accessibilityRole="button"
            accessibilityLabel={`Set rest to ${seconds} seconds`}
          >
            <Text style={styles.presetText}>{seconds}s</Text>
          </Pressable>
        ))}
      </View>

      <View style={styles.actions}>
        <Pressable
          style={styles.secondary}
          onPress={() => onAddSeconds(15)}
          accessibilityRole="button"
          accessibilityLabel="Add 15 seconds"
        >
          <Text style={styles.secondaryText}>+15s</Text>
        </Pressable>
        <Pressable
          style={styles.primary}
          onPress={onSkip}
          accessibilityRole="button"
          accessibilityLabel="Skip rest"
        >
          <Text style={styles.primaryText}>Skip rest</Text>
        </Pressable>
      </View>
    </View>
  );
}

export const DEFAULT_REST_SECONDS = 90;

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.xl,
    borderWidth: 1,
    borderColor: theme.colors.primary,
    padding: theme.spacing.md,
    marginBottom: theme.spacing.md,
    alignItems: 'center',
    gap: theme.spacing.sm,
  },
  label: { color: theme.colors.primary, fontWeight: '700', fontSize: 14 },
  time: {
    color: theme.colors.text,
    fontSize: 40,
    fontWeight: '800',
    fontVariant: ['tabular-nums'],
  },
  presets: { flexDirection: 'row', gap: 8 },
  preset: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.surfaceLight,
  },
  presetText: { color: theme.colors.textMuted, fontWeight: '600', fontSize: 13 },
  actions: { flexDirection: 'row', gap: theme.spacing.sm, width: '100%', marginTop: 4 },
  secondary: {
    flex: 1,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    paddingVertical: 12,
    alignItems: 'center',
  },
  secondaryText: { color: theme.colors.text, fontWeight: '600' },
  primary: {
    flex: 1,
    borderRadius: theme.radius.lg,
    backgroundColor: theme.colors.primary,
    paddingVertical: 12,
    alignItems: 'center',
  },
  primaryText: { color: '#fff', fontWeight: '700' },
});
