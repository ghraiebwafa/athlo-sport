import { Camera } from 'lucide-react-native';
import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

const placeholders = ['Apr 28', 'May 8', 'May 18', 'May 28'];

export function ProgressPhotosSection() {
  return (
    <View style={styles.wrap}>
      <View style={styles.header}>
        <Text style={styles.title}>Progress Photos</Text>
        <Text style={styles.link}>View All</Text>
      </View>
      <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.row}>
        {placeholders.map((date) => (
          <View key={date} style={styles.card}>
            <View style={styles.placeholder}>
              <Camera color={theme.colors.textMuted} size={24} />
            </View>
            <Text style={styles.date}>{date}</Text>
          </View>
        ))}
      </ScrollView>
      <Text style={styles.hint}>Photo tracking coming soon — capture progress over time.</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: { marginBottom: theme.spacing.lg },
  header: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: theme.spacing.sm },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  link: { color: theme.colors.primary, fontWeight: '600', fontSize: 13 },
  row: { gap: theme.spacing.sm },
  card: { width: 100 },
  placeholder: {
    height: 130,
    borderRadius: theme.radius.lg,
    backgroundColor: theme.colors.surface,
    borderWidth: 1,
    borderColor: theme.colors.border,
    alignItems: 'center',
    justifyContent: 'center',
  },
  date: { color: theme.colors.textMuted, fontSize: 11, marginTop: 6, textAlign: 'center' },
  hint: { color: theme.colors.textMuted, fontSize: 12, marginTop: theme.spacing.sm },
});
