import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { router } from 'expo-router';
import { Pencil, Plus, Trash2 } from 'lucide-react-native';
import { Alert, FlatList, Pressable, StyleSheet, Text, View } from 'react-native';
import { AdminGuard } from '@/components/auth/AdminGuard';
import { QueryState } from '@/components/ui/QueryState';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { deleteProgram } from '@/lib/api/admin';
import { getApiErrorMessage } from '@/lib/api/client';
import { getPrograms } from '@/lib/api/programs';
import { adminProgramCreate, adminProgramEdit } from '@/lib/routes';

export default function AdminProgramsScreen() {
  const queryClient = useQueryClient();
  const programsQuery = useQuery({ queryKey: ['programs'], queryFn: getPrograms });

  const deleteMutation = useMutation({
    mutationFn: deleteProgram,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['programs'] });
    },
    onError: (err) => Alert.alert('Error', getApiErrorMessage(err)),
  });

  return (
    <AdminGuard>
      <View style={styles.screen}>
        <ScreenHeader
          title="Programs"
          onBack={() => router.back()}
          right={
            <Pressable
              onPress={() => router.push(adminProgramCreate())}
              accessibilityRole="button"
              accessibilityLabel="Add program"
            >
              <Plus color={theme.colors.primary} size={24} />
            </Pressable>
          }
        />

        {programsQuery.isError ? (
          <QueryState
            message={getApiErrorMessage(programsQuery.error)}
            onRetry={() => void programsQuery.refetch()}
          />
        ) : (
          <FlatList
            data={programsQuery.data ?? []}
            keyExtractor={(item) => item.id}
            contentContainerStyle={styles.list}
            refreshing={programsQuery.isFetching}
            onRefresh={() => void programsQuery.refetch()}
            ListEmptyComponent={
              !programsQuery.isLoading ? <Text style={styles.empty}>No programs yet.</Text> : null
            }
            renderItem={({ item }) => (
              <View style={styles.row}>
                <Pressable
                  style={styles.rowBody}
                  onPress={() => router.push(adminProgramEdit(item.id))}
                  accessibilityRole="button"
                  accessibilityLabel={`Edit ${item.name}`}
                >
                  <Text style={styles.name}>{item.name}</Text>
                  <Text style={styles.meta}>
                    {item.categoryName} · {item.difficulty} · {item.durationMinutes} min ·{' '}
                    {item.exerciseCount} exercises
                    {item.isFeatured ? ' · Featured' : ''}
                  </Text>
                </Pressable>
                <Pressable
                  onPress={() => router.push(adminProgramEdit(item.id))}
                  hitSlop={8}
                  accessibilityLabel={`Edit ${item.name}`}
                >
                  <Pencil color={theme.colors.primary} size={18} />
                </Pressable>
                <Pressable
                  onPress={() =>
                    Alert.alert('Delete program?', item.name, [
                      { text: 'Cancel', style: 'cancel' },
                      {
                        text: 'Delete',
                        style: 'destructive',
                        onPress: () => deleteMutation.mutate(item.id),
                      },
                    ])
                  }
                  hitSlop={8}
                  accessibilityLabel={`Delete ${item.name}`}
                >
                  <Trash2 color={theme.colors.red} size={18} />
                </Pressable>
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
});
