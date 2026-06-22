import { Dumbbell, Flame, LucideIcon, Trophy } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import type { Achievement } from '@/lib/profileHelpers';

const icons: Record<string, LucideIcon> = {
  first: Dumbbell,
  streak7: Flame,
  workouts25: Dumbbell,
  calories10k: Trophy,
};

interface AchievementsSectionProps {
  items: Achievement[];
}

export function AchievementsSection({ items }: AchievementsSectionProps) {
  return (
    <View style={styles.section}>
      <View style={styles.header}>
        <Text style={styles.title}>Achievements</Text>
        <Text style={styles.link}>View All</Text>
      </View>
      <View style={styles.row}>
        {items.map((item) => {
          const Icon = icons[item.id] ?? Trophy;
          return (
            <View key={item.id} style={[styles.badge, !item.unlocked && styles.badgeLocked]}>
              <View style={[styles.hex, { borderColor: item.color }]}>
                <Icon color={item.unlocked ? item.color : theme.colors.textMuted} size={18} />
              </View>
              <Text style={styles.badgeTitle} numberOfLines={2}>{item.title}</Text>
            </View>
          );
        })}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  section: { marginBottom: theme.spacing.lg },
  header: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: theme.spacing.sm },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  link: { color: theme.colors.primary, fontWeight: '600', fontSize: 13 },
  row: { flexDirection: 'row', gap: theme.spacing.sm },
  badge: { flex: 1, alignItems: 'center', gap: 6 },
  badgeLocked: { opacity: 0.45 },
  hex: {
    width: 52,
    height: 52,
    borderRadius: 14,
    borderWidth: 2,
    backgroundColor: theme.colors.surface,
    alignItems: 'center',
    justifyContent: 'center',
  },
  badgeTitle: { color: theme.colors.textMuted, fontSize: 10, textAlign: 'center' },
});
