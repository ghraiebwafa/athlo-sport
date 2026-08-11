import { useInfiniteQuery } from '@tanstack/react-query';
import { router } from 'expo-router';
import { ActivityIndicator, FlatList, StyleSheet, Text, View } from 'react-native';
import { AdminGuard } from '@/components/auth/AdminGuard';
import { QueryState } from '@/components/ui/QueryState';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { getAdminUsers } from '@/lib/api/admin';
import { getApiErrorMessage } from '@/lib/api/client';

export default function AdminUsersScreen() {
  const query = useInfiniteQuery({
    queryKey: ['adminUsers'],
    queryFn: ({ pageParam }) => getAdminUsers(pageParam, 20),
    initialPageParam: 1,
    getNextPageParam: (last) => {
      const loaded = last.page * last.pageSize;
      return loaded < last.totalCount ? last.page + 1 : undefined;
    },
  });

  const users = query.data?.pages.flatMap((p) => p.items) ?? [];

  return (
    <AdminGuard>
      <View style={styles.screen}>
        <ScreenHeader title="Users" onBack={() => router.back()} />

        {query.isError ? (
          <QueryState message={getApiErrorMessage(query.error)} onRetry={() => void query.refetch()} />
        ) : (
          <FlatList
            data={users}
            keyExtractor={(item) => item.id}
            contentContainerStyle={styles.list}
            refreshing={query.isFetching && !query.isFetchingNextPage}
            onRefresh={() => void query.refetch()}
            onEndReached={() => {
              if (query.hasNextPage && !query.isFetchingNextPage) void query.fetchNextPage();
            }}
            onEndReachedThreshold={0.4}
            ListEmptyComponent={
              !query.isLoading ? <Text style={styles.empty}>No users found.</Text> : null
            }
            ListFooterComponent={
              query.isFetchingNextPage ? (
                <ActivityIndicator color={theme.colors.primary} style={{ marginVertical: 16 }} />
              ) : null
            }
            renderItem={({ item }) => (
              <View style={styles.row}>
                <View style={styles.rowBody}>
                  <Text style={styles.name}>{item.fullName}</Text>
                  <Text style={styles.meta}>{item.email}</Text>
                </View>
                <Text style={styles.role}>{item.role}</Text>
              </View>
            )}
          />
        )}
      </View>
    </AdminGuard>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  list: { padding: theme.spacing.md, paddingBottom: 40, gap: 8 },
  empty: { color: theme.colors.textMuted, textAlign: 'center', marginTop: 40 },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
  },
  rowBody: { flex: 1 },
  name: { color: theme.colors.text, fontWeight: '600', fontSize: 16 },
  meta: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
  role: { color: theme.colors.primary, fontWeight: '700', fontSize: 12 },
});
