import { CalendarDays } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import type { WeeklySummaryDto } from '@/lib/api/progress';

interface WeeklySummaryCardProps {
  summary: WeeklySummaryDto;
}

export function WeeklySummaryCard({ summary }: WeeklySummaryCardProps) {
  const deltaHint =
    summary.workoutsCompleted === 0
      ? 'Log a session to start the week'
      : `${summary.minutesTrained} min · ${summary.caloriesBurned} kcal`;

  return (
    <View style={styles.card}>
      <View style={styles.header}>
        <Text style={styles.title}>This week</Text>
        <CalendarDays color={theme.colors.primary} size={22} />
      </View>
      <Text style={styles.headline}>{summary.headline || 'Keep showing up.'}</Text>
      <Text style={styles.subtitle}>{deltaHint}</Text>
      <View style={styles.stats}>
        <Stat label="Workouts" value={String(summary.workoutsCompleted)} />
        <Stat label="Streak" value={`${summary.currentStreak}d`} />
        <Stat label="Minutes" value={String(summary.minutesTrained)} />
      </View>
    </View>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <View style={styles.stat}>
      <Text style={styles.statValue}>{value}</Text>
      <Text style={styles.statLabel}>{label}</Text>
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
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  title: { color: theme.colors.text, fontSize: 18, fontWeight: '700' },
  headline: { color: theme.colors.text, fontSize: 15, fontWeight: '600', marginBottom: 4 },
  subtitle: { color: theme.colors.textMuted, fontSize: 13, marginBottom: 14 },
  stats: { flexDirection: 'row', gap: theme.spacing.md },
  stat: { flex: 1 },
  statValue: { color: theme.colors.primary, fontSize: 20, fontWeight: '700' },
  statLabel: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
});
