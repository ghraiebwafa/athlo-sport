import { useQuery } from '@tanstack/react-query';
import { router, useLocalSearchParams } from 'expo-router';
import { useMemo } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { QueryState } from '@/components/ui/QueryState';
import { SummaryStat } from '@/components/workout/SummaryStat';
import { theme } from '@/constants/theme';
import { getApiErrorMessage } from '@/lib/api/client';
import { getWorkoutHistorySession } from '@/lib/api/workouts';
import { normalizeRouteParam } from '@/lib/routeParams';
import { formatHms } from '@/lib/workoutTimer';
import type { WorkoutSetLog } from '@/lib/types';
import { Clock, Flame } from 'lucide-react-native';

function groupSetsByExercise(sets: WorkoutSetLog[]) {
  const groups = new Map<string, WorkoutSetLog[]>();
  for (const set of sets) {
    const key = set.exerciseName || set.exerciseId;
    const existing = groups.get(key) ?? [];
    existing.push(set);
    groups.set(key, existing);
  }
  return Array.from(groups.entries()).map(([name, logs]) => ({
    name,
    logs: logs.sort((a, b) => a.setNumber - b.setNumber),
  }));
}

export default function WorkoutHistoryDetailScreen() {
  const rawId = useLocalSearchParams<{ id: string | string[] }>().id;
  const id = normalizeRouteParam(rawId);

  const sessionQuery = useQuery({
    queryKey: ['workoutHistory', id],
    queryFn: () => getWorkoutHistorySession(id!),
    enabled: !!id,
  });

  const groupedSets = useMemo(
    () => groupSetsByExercise(sessionQuery.data?.sets ?? []),
    [sessionQuery.data?.sets]
  );

  if (!id) {
    return (
      <View style={styles.screen}>
        <ScreenHeader title="Workout" onBack={() => router.back()} />
        <QueryState message="Workout not found." />
      </View>
    );
  }

  const session = sessionQuery.data;
  const completedDate = session?.completedAt
    ? new Date(session.completedAt).toLocaleString(undefined, {
        weekday: 'long',
        month: 'short',
        day: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
      })
    : null;

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Workout Detail" onBack={() => router.back()} />

      {sessionQuery.isLoading ? (
        <View style={styles.centered}>
          <ActivityIndicator color={theme.colors.primary} size="large" />
        </View>
      ) : sessionQuery.isError ? (
        <QueryState
          message={getApiErrorMessage(sessionQuery.error)}
          onRetry={() => void sessionQuery.refetch()}
        />
      ) : !session ? (
        <QueryState message="Workout not found." />
      ) : (
        <ScrollView contentContainerStyle={styles.content}>
          <Text style={styles.programName}>{session.programName}</Text>
          {completedDate ? <Text style={styles.date}>{completedDate}</Text> : null}

          <View style={styles.statsRow}>
            <SummaryStat
              icon={Clock}
              iconColor={theme.colors.primary}
              value={
                session.durationMinutes != null
                  ? formatHms(session.durationMinutes * 60)
                  : '—'
              }
              label="Duration"
            />
            <SummaryStat
              icon={Flame}
              iconColor={theme.colors.orange}
              value={session.caloriesBurned != null ? String(session.caloriesBurned) : '—'}
              label="Calories"
              sublabel="kcal"
            />
          </View>

          <Text style={styles.sectionTitle}>Logged sets</Text>
          {groupedSets.length === 0 ? (
            <Text style={styles.emptySets}>No sets were logged for this session.</Text>
          ) : (
            groupedSets.map((group) => (
              <View key={group.name} style={styles.exerciseCard}>
                <Text style={styles.exerciseName}>{group.name}</Text>
                {group.logs.map((set) => (
                  <View key={set.id} style={styles.setRow}>
                    <Text style={styles.setLabel}>Set {set.setNumber}</Text>
                    <Text style={styles.setMeta}>
                      {set.repsCompleted} reps
                      {set.weightKg != null ? ` · ${set.weightKg} kg` : ''}
                      {!set.completed ? ' · skipped' : ''}
                    </Text>
                  </View>
                ))}
              </View>
            ))
          )}
        </ScrollView>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  centered: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  content: { padding: theme.spacing.md, paddingBottom: 40 },
  programName: { color: theme.colors.text, fontSize: 22, fontWeight: '800' },
  date: { color: theme.colors.textMuted, marginTop: 4, marginBottom: theme.spacing.md },
  statsRow: { flexDirection: 'row', gap: theme.spacing.sm, marginBottom: theme.spacing.lg },
  sectionTitle: {
    color: theme.colors.text,
    fontWeight: '700',
    fontSize: 16,
    marginBottom: theme.spacing.sm,
  },
  emptySets: { color: theme.colors.textMuted },
  exerciseCard: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
    marginBottom: theme.spacing.sm,
  },
  exerciseName: { color: theme.colors.text, fontWeight: '700', marginBottom: theme.spacing.sm },
  setRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: 6,
    borderTopWidth: 1,
    borderTopColor: theme.colors.border,
  },
  setLabel: { color: theme.colors.textMuted, fontWeight: '600' },
  setMeta: { color: theme.colors.text, fontWeight: '600' },
});
