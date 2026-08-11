import { useQuery } from '@tanstack/react-query';
import { router } from 'expo-router';
import { useCallback, useMemo } from 'react';
import { ActivityIndicator, FlatList, StyleSheet, Text, View } from 'react-native';
import { PopularWorkoutRow } from '@/components/programs/PopularWorkoutRow';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { QueryState } from '@/components/ui/QueryState';
import { theme } from '@/constants/theme';
import { getApiErrorMessage } from '@/lib/api/client';
import { getPrograms } from '@/lib/api/programs';
import { getSavedProgramIds } from '@/lib/savedPrograms';
import { ROUTES } from '@/lib/routes';
import type { ProgramListItem } from '@/lib/types';

export default function SavedProgramsScreen() {
  const programsQuery = useQuery({ queryKey: ['programs'], queryFn: getPrograms });
  const savedQuery = useQuery({ queryKey: ['savedPrograms'], queryFn: getSavedProgramIds });

  const saved = useMemo(() => {
    const ids = new Set(savedQuery.data ?? []);
    return (programsQuery.data ?? []).filter((p) => ids.has(p.id));
  }, [programsQuery.data, savedQuery.data]);

  const renderItem = useCallback(
    ({ item }: { item: ProgramListItem }) => <PopularWorkoutRow program={item} />,
    []
  );

  const isLoading = programsQuery.isLoading || savedQuery.isLoading;
  const isError = programsQuery.isError || savedQuery.isError;

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Saved Programs" onBack={() => router.back()} />

      {isLoading ? (
        <View style={styles.centered}>
          <ActivityIndicator color={theme.colors.primary} size="large" />
        </View>
      ) : isError ? (
        <QueryState
          message={getApiErrorMessage(programsQuery.error ?? savedQuery.error)}
          onRetry={() => {
            void programsQuery.refetch();
            void savedQuery.refetch();
          }}
        />
      ) : saved.length === 0 ? (
        <QueryState
          message="No saved programs yet. Bookmark a program from its detail page."
          onRetry={() => router.push(ROUTES.programs)}
          retryLabel="Browse Programs"
        />
      ) : (
        <FlatList
          data={saved}
          keyExtractor={(item) => item.id}
          renderItem={renderItem}
          contentContainerStyle={styles.list}
          ListHeaderComponent={
            <Text style={styles.count}>{saved.length} saved</Text>
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
  count: { color: theme.colors.textMuted, marginBottom: theme.spacing.sm },
});
