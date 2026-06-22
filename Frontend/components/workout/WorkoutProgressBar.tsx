import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface WorkoutProgressBarProps {
  percent: number;
}

export function WorkoutProgressBar({ percent }: WorkoutProgressBarProps) {
  const clamped = Math.min(100, Math.max(0, percent));
  return (
    <View style={styles.wrap}>
      <View style={styles.header}>
        <Text style={styles.title}>Workout Progress</Text>
        <Text style={styles.percent}>{clamped}%</Text>
      </View>
      <View style={styles.track}>
        <View style={[styles.fill, { width: `${clamped}%` }]} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: { marginBottom: theme.spacing.lg },
  header: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 8 },
  title: { color: theme.colors.text, fontWeight: '600', fontSize: 15 },
  percent: { color: theme.colors.primary, fontWeight: '700' },
  track: {
    height: 8,
    backgroundColor: theme.colors.surfaceLight,
    borderRadius: theme.radius.full,
    overflow: 'hidden',
  },
  fill: { height: '100%', backgroundColor: theme.colors.primary, borderRadius: theme.radius.full },
});
