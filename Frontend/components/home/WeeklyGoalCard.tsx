import { Target } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface WeeklyGoalCardProps {
  completed: number;
  goal?: number;
}

export function WeeklyGoalCard({ completed, goal = 5 }: WeeklyGoalCardProps) {
  const percent = Math.min(100, Math.round((completed / goal) * 100));

  return (
    <View style={styles.card}>
      <View style={styles.header}>
        <Text style={styles.title}>Weekly Goal</Text>
        <Target color={theme.colors.primary} size={22} />
      </View>
      <Text style={styles.subtitle}>
        {completed} of {goal} workouts completed
      </Text>
      <View style={styles.barRow}>
        <View style={styles.track}>
          <View style={[styles.fill, { width: `${percent}%` }]} />
        </View>
        <Text style={styles.percent}>{percent}%</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginBottom: theme.spacing.md,
  },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  title: { color: theme.colors.text, fontSize: 18, fontWeight: '700' },
  subtitle: { color: theme.colors.textMuted, fontSize: 14, marginBottom: 12 },
  barRow: { flexDirection: 'row', alignItems: 'center', gap: 12 },
  track: {
    flex: 1,
    height: 8,
    backgroundColor: theme.colors.surfaceLight,
    borderRadius: theme.radius.full,
    overflow: 'hidden',
  },
  fill: { height: '100%', backgroundColor: theme.colors.primary, borderRadius: theme.radius.full },
  percent: { color: theme.colors.primary, fontWeight: '700', fontSize: 14 },
});
