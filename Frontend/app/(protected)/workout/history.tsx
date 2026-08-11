import { useInfiniteQuery } from '@tanstack/react-query';
import { router } from 'expo-router';
import { useCallback } from 'react';
import {
  ActivityIndicator,
  FlatList,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { QueryState } from '@/components/ui/QueryState';
import { theme } from '@/constants/theme';
import { getApiErrorMessage } from '@/lib/api/client';
import { getWorkoutHistory } from '@/lib/api/workouts';
import type { WorkoutSession } from '@/lib/types';

function HistoryRow({ item }: { item: WorkoutSession }) {
  const date = item.completedAt
    ? new Date(item.completedAt).toLocaleDateString(undefined, {
        weekday: 'short',
        month: 'short',
        day: 'numeric',
      })
    : '—';

  return (
    <View style={styles.row} accessibilityRole="summary">
      <View style={styles.rowInfo}>
        <Text style={styles.rowTitle}>{item.programName}</Text>
        <Text style={styles.rowMeta}>
          {date}
          {item.durationMinutes != null ? ` · ${item.durationMinutes} min` : ''}
        </Text>
      </View>
      <Text style={styles.rowCal}>
        {item.caloriesBurned != null ? `${item.caloriesBurned} cal` : '—'}
      </Text>
    </View>
  );
}

export default function WorkoutHistoryScreen() {
  const historyQuery = useInfiniteQuery({
    queryKey: ['workoutHistory'],
    queryFn: ({ pageParam }) => getWorkoutHistory(pageParam, 20),
    initialPageParam: 1,
    getNextPageParam: (last) =>
      last.page < last.totalPages ? last.page + 1 : undefined,
  });

  const items = historyQuery.data?.pages.flatMap((p) => p.items) ?? [];

  const renderItem = useCallback(
    ({ item }: { item: WorkoutSession }) => <HistoryRow item={item} />,
    []
  );

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Workout History" onBack={() => router.back()} />

      {historyQuery.isLoading ? (
        <View style={styles.centered}>
          <ActivityIndicator color={theme.colors.primary} size="large" />
        </View>
      ) : historyQuery.isError ? (
        <QueryState
          message={getApiErrorMessage(historyQuery.error)}
          onRetry={() => historyQuery.refetch()}
        />
      ) : items.length === 0 ? (
        <QueryState message="No completed workouts yet. Start a program to build your history." />
      ) : (
        <FlatList
          data={items}
          keyExtractor={(item) => item.id}
          renderItem={renderItem}
          contentContainerStyle={styles.list}
          onEndReached={() => {
            if (historyQuery.hasNextPage && !historyQuery.isFetchingNextPage) {
              void historyQuery.fetchNextPage();
            }
          }}
          onEndReachedThreshold={0.4}
          ListFooterComponent={
            historyQuery.isFetchingNextPage ? (
              <ActivityIndicator color={theme.colors.primary} style={styles.footer} />
            ) : null
          }
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  centered: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  list: { padding: theme.spacing.md, paddingBottom: 40 },
  row: {
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
  rowInfo: { flex: 1, paddingRight: theme.spacing.sm },
  rowTitle: { color: theme.colors.text, fontWeight: '600' },
  rowMeta: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
  rowCal: { color: theme.colors.primary, fontWeight: '600' },
  footer: { marginVertical: theme.spacing.md },
});
