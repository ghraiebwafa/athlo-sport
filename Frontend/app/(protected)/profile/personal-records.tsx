import { useQuery } from '@tanstack/react-query';
import { router } from 'expo-router';
import { ActivityIndicator, FlatList, StyleSheet, Text, View } from 'react-native';
import { QueryState } from '@/components/ui/QueryState';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { getApiErrorMessage } from '@/lib/api/client';
import { getProgress } from '@/lib/api/progress';
import { buildAllPersonalRecords } from '@/lib/profileHelpers';

export default function PersonalRecordsScreen() {
  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['progress'],
    queryFn: getProgress,
  });

  if (isLoading) {
    return (
      <View style={styles.screen}>
        <ScreenHeader title="Personal Records" onBack={() => router.back()} />
        <View style={styles.centered}>
          <ActivityIndicator color={theme.colors.primary} size="large" />
        </View>
      </View>
    );
  }

  if (isError || !data) {
    return (
      <View style={styles.screen}>
        <ScreenHeader title="Personal Records" onBack={() => router.back()} />
        <QueryState
          message={getApiErrorMessage(error) || 'Unable to load records.'}
          onRetry={() => void refetch()}
        />
      </View>
    );
  }

  const records = buildAllPersonalRecords(data);
  const hasLifts = (data.personalRecords?.length ?? 0) > 0;

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Personal Records" onBack={() => router.back()} />
      <FlatList
        data={records}
        keyExtractor={(item) => item.id}
        contentContainerStyle={styles.list}
        ListHeaderComponent={
          <Text style={styles.summary}>
            {hasLifts
              ? `${records.length} lift PR${records.length === 1 ? '' : 's'} from weighted sets`
              : 'Session highlights until you log weighted sets'}
          </Text>
        }
        ListEmptyComponent={<Text style={styles.empty}>No records yet.</Text>}
        renderItem={({ item }) => (
          <View style={styles.row}>
            <View style={[styles.icon, { backgroundColor: `${item.color}22` }]}>
              <View style={[styles.dot, { backgroundColor: item.color }]} />
            </View>
            <View style={styles.text}>
              <Text style={styles.label}>{item.label}</Text>
              <Text style={styles.value}>{item.value}</Text>
              {item.achievedAt ? (
                <Text style={styles.meta}>{new Date(item.achievedAt).toLocaleDateString()}</Text>
              ) : null}
            </View>
          </View>
        )}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  centered: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  list: { padding: theme.spacing.md, paddingBottom: 40, gap: 8 },
  summary: { color: theme.colors.textMuted, marginBottom: theme.spacing.sm },
  empty: { color: theme.colors.textMuted, textAlign: 'center', marginTop: 40 },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  icon: {
    width: 40,
    height: 40,
    borderRadius: 20,
    alignItems: 'center',
    justifyContent: 'center',
  },
  dot: { width: 10, height: 10, borderRadius: 5 },
  text: { flex: 1 },
  label: { color: theme.colors.textMuted, fontSize: 12 },
  value: { color: theme.colors.text, fontWeight: '700', fontSize: 16, marginTop: 2 },
  meta: { color: theme.colors.textMuted, fontSize: 11, marginTop: 2 },
});
