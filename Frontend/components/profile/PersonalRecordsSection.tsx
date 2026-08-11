import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import type { PersonalRecord } from '@/lib/profileHelpers';

interface PersonalRecordsSectionProps {
  records: PersonalRecord[];
}

export function PersonalRecordsSection({ records }: PersonalRecordsSectionProps) {
  return (
    <View style={styles.section}>
      <View style={styles.header}>
        <Text style={styles.title}>Personal Records</Text>
      </View>
      {records.length === 0 ? (
        <Text style={styles.empty}>Log weighted sets to unlock lift PRs.</Text>
      ) : (
        records.map((record) => (
          <View key={record.id} style={styles.row}>
            <View style={[styles.icon, { backgroundColor: `${record.color}22` }]}>
              <View style={[styles.dot, { backgroundColor: record.color }]} />
            </View>
            <View style={styles.text}>
              <Text style={styles.label}>{record.label}</Text>
              <Text style={styles.value}>{record.value}</Text>
            </View>
          </View>
        ))
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  section: { marginBottom: theme.spacing.lg },
  header: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: theme.spacing.sm },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  empty: { color: theme.colors.textMuted, fontSize: 13, marginBottom: theme.spacing.sm },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    marginBottom: theme.spacing.sm,
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
});
