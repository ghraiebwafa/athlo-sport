import { Activity, Dumbbell, LucideIcon, Target } from 'lucide-react-native';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import type { FitnessGoal } from '@/lib/types';

const goals: { value: FitnessGoal; label: string; icon: LucideIcon }[] = [
  { value: 'LoseWeight', label: 'Lose Weight', icon: Target },
  { value: 'BuildMuscle', label: 'Build Muscle', icon: Dumbbell },
  { value: 'StayActive', label: 'Stay Active', icon: Activity },
];

interface GoalSelectorProps {
  value: FitnessGoal;
  onChange: (goal: FitnessGoal) => void;
}

export function GoalSelector({ value, onChange }: GoalSelectorProps) {
  return (
    <View style={styles.wrap}>
      <Text style={styles.label}>Fitness Goal</Text>
      <View style={styles.row}>
        {goals.map((g) => {
          const active = value === g.value;
          const Icon = g.icon;
          return (
            <Pressable
              key={g.value}
              onPress={() => onChange(g.value)}
              style={[styles.card, active && styles.cardActive]}
            >
              <Icon color={active ? theme.colors.primary : theme.colors.textMuted} size={20} />
              <Text style={[styles.cardLabel, active && styles.cardLabelActive]}>{g.label}</Text>
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: { gap: 8 },
  label: { color: theme.colors.textMuted, fontSize: 14, fontWeight: '500' },
  row: { flexDirection: 'row', gap: theme.spacing.sm },
  card: {
    flex: 1,
    alignItems: 'center',
    gap: 8,
    paddingVertical: 14,
    paddingHorizontal: 6,
    borderRadius: theme.radius.lg,
    backgroundColor: theme.colors.surface,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  cardActive: { borderColor: theme.colors.primary },
  cardLabel: { color: theme.colors.textMuted, fontSize: 11, fontWeight: '600', textAlign: 'center' },
  cardLabelActive: { color: theme.colors.primary },
});
