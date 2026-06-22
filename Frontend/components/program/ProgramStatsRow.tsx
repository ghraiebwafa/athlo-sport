import { Calendar, Clock, Flame, LucideIcon, Signal } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface StatProps {
  icon: LucideIcon;
  value: string;
  label: string;
}

function Stat({ icon: Icon, value, label }: StatProps) {
  return (
    <View style={styles.stat}>
      <Icon color={theme.colors.primary} size={18} />
      <Text style={styles.value}>{value}</Text>
      <Text style={styles.label}>{label}</Text>
    </View>
  );
}

interface ProgramStatsRowProps {
  durationMinutes: number;
  difficulty: string;
  calories: number;
  exerciseCount: number;
}

export function ProgramStatsRow({ durationMinutes, difficulty, calories, exerciseCount }: ProgramStatsRowProps) {
  return (
    <View style={styles.row}>
      <Stat icon={Clock} value={`${durationMinutes} min`} label="Duration" />
      <Stat icon={Signal} value={difficulty} label="Difficulty" />
      <Stat icon={Flame} value={`${calories}`} label="Calories" />
      <Stat icon={Calendar} value={String(exerciseCount)} label="Exercises" />
    </View>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: theme.spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.border,
    marginBottom: theme.spacing.lg,
  },
  stat: { flex: 1, alignItems: 'center', gap: 4 },
  value: { color: theme.colors.text, fontWeight: '700', fontSize: 13, textAlign: 'center' },
  label: { color: theme.colors.textMuted, fontSize: 10, textAlign: 'center' },
});
