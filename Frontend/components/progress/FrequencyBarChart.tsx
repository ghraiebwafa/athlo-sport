import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface FrequencyBarChartProps {
  data: { weekStart: string; workoutCount: number }[];
  totalWorkouts: number;
}

export function FrequencyBarChart({ data, totalWorkouts }: FrequencyBarChartProps) {
  const max = Math.max(1, ...data.map((d) => d.workoutCount));

  return (
    <View style={styles.wrap}>
      <View style={styles.header}>
        <Text style={styles.title}>Workout Frequency</Text>
        <Text style={styles.total}>{totalWorkouts} workouts</Text>
      </View>
      {data.length === 0 ? (
        <Text style={styles.empty}>No workout data for this period.</Text>
      ) : (
        <View style={styles.chart}>
          {data.map((item) => {
            const height = `${Math.max(8, (item.workoutCount / max) * 100)}%` as `${number}%`;
            const label = new Date(item.weekStart).toLocaleDateString(undefined, {
              month: 'short',
              day: 'numeric',
            });
            return (
              <View key={item.weekStart} style={styles.barCol}>
                <View style={styles.barTrack}>
                  <View style={[styles.barFill, { height }]} />
                </View>
                <Text style={styles.barLabel}>{label}</Text>
              </View>
            );
          })}
        </View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginBottom: theme.spacing.lg,
  },
  header: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: theme.spacing.md },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  total: { color: theme.colors.primary, fontWeight: '600', fontSize: 14 },
  chart: { flexDirection: 'row', alignItems: 'flex-end', height: 120, gap: 8 },
  barCol: { flex: 1, alignItems: 'center', height: '100%', justifyContent: 'flex-end' },
  barTrack: {
    width: '100%',
    height: 90,
    justifyContent: 'flex-end',
    backgroundColor: theme.colors.surfaceLight,
    borderRadius: theme.radius.sm,
    overflow: 'hidden',
  },
  barFill: { width: '100%', backgroundColor: theme.colors.primary, borderRadius: theme.radius.sm },
  barLabel: { color: theme.colors.textMuted, fontSize: 9, marginTop: 6, textAlign: 'center' },
  empty: { color: theme.colors.textMuted, textAlign: 'center', paddingVertical: theme.spacing.lg },
});
