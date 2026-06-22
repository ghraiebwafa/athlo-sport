import { useQuery } from '@tanstack/react-query';
import { Calendar, Clock, Dumbbell, Flame, Trophy } from 'lucide-react-native';
import { useMemo, useState } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';
import { FrequencyBarChart } from '@/components/progress/FrequencyBarChart';
import { MuscleGroupChart } from '@/components/progress/MuscleGroupChart';
import { OverviewCard } from '@/components/progress/OverviewCard';
import { ProgressPhotosSection } from '@/components/progress/ProgressPhotosSection';
import { TimeRangeFilter } from '@/components/progress/TimeRangeFilter';
import { theme } from '@/constants/theme';
import { getApiErrorMessage } from '@/lib/api/client';
import { getProgress } from '@/lib/api/progress';
import {
  filterRecentWorkouts,
  filterWeeklyFrequency,
  formatDuration,
  muscleGroupBreakdown,
  splitWeeklyComparison,
  topMuscleGroup,
  totalDurationMinutes,
  trendPercent,
  type TimeRange,
} from '@/lib/progressFilters';

export default function ProgressScreen() {
  const [range, setRange] = useState<TimeRange>('1M');
  const { data, isLoading, isError, error } = useQuery({ queryKey: ['progress'], queryFn: getProgress });

  const filtered = useMemo(() => {
    if (!data) return null;
    const recent = filterRecentWorkouts(data.recentWorkouts, range);
    const weekly = filterWeeklyFrequency(data.weeklyFrequency, range);
    const durationMin = totalDurationMinutes(recent);
    const calories = recent.reduce((s, w) => s + w.caloriesBurned, 0);
    const workouts = range === 'All' ? data.totalWorkouts : recent.length || weekly.reduce((s, w) => s + w.workoutCount, 0);
    const weeklyCompare = splitWeeklyComparison(data.weeklyFrequency);
    const muscles = muscleGroupBreakdown(recent.map((w) => w.programName));

    return {
      recent,
      weekly,
      durationMin,
      calories: range === 'All' ? data.totalCaloriesBurned : calories || data.totalCaloriesBurned,
      workouts: range === 'All' ? data.totalWorkouts : workouts,
      personalBests: data.personalBests,
      streak: data.currentStreak,
      goalProgress: data.goalProgressPercent,
      weightLine: `${data.currentWeight} kg → ${data.goalWeight} kg`,
      workoutTrend: trendPercent(weeklyCompare.current, weeklyCompare.previous),
      muscles,
      mostTrained: topMuscleGroup(muscles),
    };
  }, [data, range]);

  if (isLoading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator color={theme.colors.primary} size="large" />
      </View>
    );
  }

  if (isError || !data || !filtered) {
    return (
      <View style={styles.centered}>
        <Text style={styles.error}>{getApiErrorMessage(error)}</Text>
      </View>
    );
  }

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <View style={styles.header}>
        <Text style={styles.heading}>Progress</Text>
        <View style={styles.calendarBtn}>
          <Calendar color={theme.colors.primary} size={22} />
        </View>
      </View>

      <TimeRangeFilter value={range} onChange={setRange} />

      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.overviewScroll}>
        <OverviewCard
          icon={Clock}
          iconColor={theme.colors.primary}
          value={formatDuration(filtered.durationMin)}
          label="Training Time"
          trend={filtered.workoutTrend}
        />
        <OverviewCard
          icon={Flame}
          iconColor={theme.colors.orange}
          value={filtered.calories.toLocaleString()}
          label="Calories Burned"
          trend={filtered.workoutTrend}
        />
        <OverviewCard
          icon={Dumbbell}
          iconColor={theme.colors.green}
          value={String(filtered.workouts)}
          label="Workouts"
          trend={filtered.workoutTrend}
        />
        <OverviewCard
          icon={Trophy}
          iconColor={theme.colors.purple}
          value={String(filtered.personalBests)}
          label="Personal Bests"
        />
      </ScrollView>

      <FrequencyBarChart
        data={filtered.weekly}
        totalWorkouts={filtered.weekly.reduce((s, w) => s + w.workoutCount, 0)}
      />

      <MuscleGroupChart data={filtered.muscles} mostTrained={filtered.mostTrained} />

      <View style={styles.goalCard}>
        <Text style={styles.goalTitle}>Goal Progress</Text>
        <Text style={styles.goalValue}>{Math.round(filtered.goalProgress)}%</Text>
        <Text style={styles.goalMeta}>{filtered.weightLine} · {filtered.streak} day streak</Text>
        <View style={styles.goalTrack}>
          <View style={[styles.goalFill, { width: `${Math.min(100, filtered.goalProgress)}%` }]} />
        </View>
      </View>

      {filtered.recent.length > 0 ? (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Recent Workouts</Text>
          {filtered.recent.map((w) => (
            <View key={w.sessionId} style={styles.workoutRow}>
              <View>
                <Text style={styles.workoutName}>{w.programName}</Text>
                <Text style={styles.workoutMeta}>
                  {new Date(w.completedAt).toLocaleDateString()} · {w.durationMinutes} min
                </Text>
              </View>
              <Text style={styles.workoutCal}>{w.caloriesBurned} cal</Text>
            </View>
          ))}
        </View>
      ) : null}

      <ProgressPhotosSection />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: theme.colors.background },
  content: { padding: theme.spacing.md, paddingBottom: theme.spacing.xl },
  centered: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: theme.colors.background },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: theme.spacing.md },
  heading: { fontSize: 28, fontWeight: '700', color: theme.colors.text },
  calendarBtn: {
    width: 40,
    height: 40,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.surface,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  overviewScroll: { marginVertical: theme.spacing.md },
  goalCard: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginBottom: theme.spacing.lg,
  },
  goalTitle: { color: theme.colors.textMuted, fontSize: 13 },
  goalValue: { color: theme.colors.primary, fontSize: 32, fontWeight: '800', marginVertical: 4 },
  goalMeta: { color: theme.colors.textMuted, fontSize: 13, marginBottom: 12 },
  goalTrack: {
    height: 8,
    backgroundColor: theme.colors.surfaceLight,
    borderRadius: theme.radius.full,
    overflow: 'hidden',
  },
  goalFill: { height: '100%', backgroundColor: theme.colors.primary },
  section: { marginBottom: theme.spacing.lg },
  sectionTitle: { fontSize: 18, fontWeight: '700', color: theme.colors.text, marginBottom: theme.spacing.sm },
  workoutRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.md,
    padding: theme.spacing.md,
    marginBottom: theme.spacing.sm,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  workoutName: { color: theme.colors.text, fontWeight: '600' },
  workoutMeta: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
  workoutCal: { color: theme.colors.primary, fontWeight: '600' },
  error: { color: theme.colors.error },
});
