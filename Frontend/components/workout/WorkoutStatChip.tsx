import { LucideIcon } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface WorkoutStatChipProps {
  icon: LucideIcon;
  iconColor: string;
  value: string;
  unit?: string;
  label: string;
  valueColor?: string;
}

export function WorkoutStatChip({
  icon: Icon,
  iconColor,
  value,
  unit,
  label,
  valueColor = theme.colors.text,
}: WorkoutStatChipProps) {
  return (
    <View style={styles.chip}>
      <Icon color={iconColor} size={18} />
      <Text style={[styles.value, { color: valueColor }]}>
        {value}
        {unit ? <Text style={styles.unit}> {unit}</Text> : null}
      </Text>
      <Text style={styles.label}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  chip: {
    flex: 1,
    alignItems: 'center',
    gap: 4,
    paddingVertical: theme.spacing.sm,
  },
  value: { fontSize: 18, fontWeight: '700', fontVariant: ['tabular-nums'] },
  unit: { fontSize: 11, fontWeight: '500', color: theme.colors.textMuted },
  label: { color: theme.colors.textMuted, fontSize: 10, textAlign: 'center' },
});
