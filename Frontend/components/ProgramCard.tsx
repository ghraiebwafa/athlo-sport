import { Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import type { ProgramListItem } from '@/lib/types';

interface ProgramCardProps {
  program: ProgramListItem;
  onPress: () => void;
}

export function ProgramCard({ program, onPress }: ProgramCardProps) {
  return (
    <Pressable onPress={onPress} style={({ pressed }) => [styles.card, pressed && styles.pressed]}>
      {program.isFeatured ? (
        <View style={styles.badge}>
          <Text style={styles.badgeText}>Featured</Text>
        </View>
      ) : null}
      <Text style={styles.category}>{program.categoryName}</Text>
      <Text style={styles.title}>{program.name}</Text>
      <Text style={styles.description} numberOfLines={2}>
        {program.description}
      </Text>
      <View style={styles.meta}>
        <Text style={styles.metaText}>{program.durationMinutes} min</Text>
        <Text style={styles.metaText}>•</Text>
        <Text style={styles.metaText}>{program.difficulty}</Text>
        <Text style={styles.metaText}>•</Text>
        <Text style={styles.metaText}>{program.estimatedCalories} cal</Text>
      </View>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    marginBottom: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  pressed: { opacity: 0.9 },
  badge: {
    alignSelf: 'flex-start',
    backgroundColor: theme.colors.primaryDark,
    borderRadius: theme.radius.full,
    paddingHorizontal: 10,
    paddingVertical: 4,
    marginBottom: 8,
  },
  badgeText: { color: '#fff', fontSize: 11, fontWeight: '700' },
  category: { color: theme.colors.primary, fontSize: 12, fontWeight: '600', marginBottom: 4 },
  title: { color: theme.colors.text, fontSize: 18, fontWeight: '700', marginBottom: 6 },
  description: { color: theme.colors.textMuted, fontSize: 14, lineHeight: 20, marginBottom: 12 },
  meta: { flexDirection: 'row', gap: 6, alignItems: 'center' },
  metaText: { color: theme.colors.textMuted, fontSize: 12 },
});
