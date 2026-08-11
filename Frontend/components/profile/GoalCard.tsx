import { Target } from 'lucide-react-native';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import { formatFitnessGoal, kgToGo } from '@/lib/profileHelpers';
import type { UserProfile } from '@/lib/types';

interface GoalCardProps {
  user: UserProfile;
  onEdit?: () => void;
}

export function GoalCard({ user, onEdit }: GoalCardProps) {
  const goalText = formatFitnessGoal(user.fitnessGoal, user.currentWeight, user.goalWeight);
  const remaining = kgToGo(user.currentWeight, user.goalWeight, user.fitnessGoal);
  const progress = Math.round(user.goalProgressPercent);

  return (
    <View style={styles.card}>
      <View style={styles.header}>
        <View style={styles.titleRow}>
          <Target color={theme.colors.primary} size={18} />
          <Text style={styles.title}>My Goal</Text>
        </View>
        {onEdit ? (
          <Pressable onPress={onEdit} accessibilityRole="button" accessibilityLabel="Edit goals">
            <Text style={styles.edit}>Edit Goal</Text>
          </Pressable>
        ) : null}
      </View>

      <View style={styles.stats}>
        <View style={styles.stat}>
          <Text style={styles.statLabel}>Goal</Text>
          <Text style={styles.statValue}>{goalText}</Text>
        </View>
        <View style={styles.stat}>
          <Text style={styles.statLabel}>Current</Text>
          <Text style={[styles.statValue, styles.highlight]}>{user.currentWeight} kg</Text>
        </View>
        <View style={styles.stat}>
          <Text style={styles.statLabel}>Target</Text>
          <Text style={[styles.statValue, styles.highlight]}>{user.goalWeight} kg</Text>
        </View>
      </View>

      <View style={styles.track}>
        <View style={[styles.fill, { width: `${Math.min(100, progress)}%` }]} />
      </View>
      <View style={styles.footer}>
        <Text style={styles.footerText}>{progress}% of goal</Text>
        <Text style={styles.footerText}>{remaining.toFixed(1)} kg to go</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginBottom: theme.spacing.lg,
  },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: theme.spacing.md },
  titleRow: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  edit: { color: theme.colors.primary, fontWeight: '600', fontSize: 13 },
  stats: { flexDirection: 'row', marginBottom: theme.spacing.md },
  stat: { flex: 1 },
  statLabel: { color: theme.colors.textMuted, fontSize: 11, marginBottom: 4 },
  statValue: { color: theme.colors.text, fontWeight: '600', fontSize: 13 },
  highlight: { color: theme.colors.primary },
  track: {
    height: 8,
    backgroundColor: theme.colors.surfaceLight,
    borderRadius: theme.radius.full,
    overflow: 'hidden',
  },
  fill: { height: '100%', backgroundColor: theme.colors.primary },
  footer: { flexDirection: 'row', justifyContent: 'space-between', marginTop: 8 },
  footerText: { color: theme.colors.textMuted, fontSize: 12 },
});
