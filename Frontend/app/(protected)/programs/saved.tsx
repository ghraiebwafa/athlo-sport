import { useQuery } from '@tanstack/react-query';
import { router } from 'expo-router';
import { useCallback } from 'react';
import { ActivityIndicator, FlatList, StyleSheet, Text, View } from 'react-native';
import { PopularWorkoutRow } from '@/components/programs/PopularWorkoutRow';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { QueryState } from '@/components/ui/QueryState';
import { theme } from '@/constants/theme';
import { getApiErrorMessage } from '@/lib/api/client';
import { listSavedPrograms } from '@/lib/savedPrograms';
import { ROUTES } from '@/lib/routes';
import type { ProgramListItem } from '@/lib/types';

export default function SavedProgramsScreen() {
  const savedQuery = useQuery({ queryKey: ['savedPrograms'], queryFn: listSavedPrograms });

  const renderItem = useCallback(
    ({ item }: { item: ProgramListItem }) => <PopularWorkoutRow program={item} />,
    []
  );

  const saved = savedQuery.data ?? [];

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Saved Programs" onBack={() => router.back()} />

      {savedQuery.isLoading ? (
        <View style={styles.centered}>
          <ActivityIndicator color={theme.colors.primary} size="large" />
        </View>
      ) : savedQuery.isError ? (
        <QueryState
          message={getApiErrorMessage(savedQuery.error)}
          onRetry={() => void savedQuery.refetch()}
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
