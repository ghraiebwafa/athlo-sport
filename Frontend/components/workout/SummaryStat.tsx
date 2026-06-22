import { LucideIcon } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface SummaryStatProps {
  icon: LucideIcon;
  iconColor: string;
  value: string;
  label: string;
  sublabel?: string;
}

export function SummaryStat({ icon: Icon, iconColor, value, label, sublabel }: SummaryStatProps) {
  return (
    <View style={styles.tile}>
      <Icon color={iconColor} size={20} />
      <Text style={[styles.value, { color: iconColor }]}>{value}</Text>
      <Text style={styles.label}>{label}</Text>
      {sublabel ? <Text style={styles.sublabel}>{sublabel}</Text> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  tile: {
    width: '48%',
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    gap: 4,
    marginBottom: theme.spacing.sm,
  },
  value: { fontSize: 22, fontWeight: '800', marginTop: 4 },
  label: { color: theme.colors.textMuted, fontSize: 12 },
  sublabel: { color: theme.colors.textMuted, fontSize: 11 },
});
