import { LucideIcon } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface StatTileProps {
  icon: LucideIcon;
  iconColor: string;
  label: string;
  value: string;
  unit?: string;
}

export function StatTile({ icon: Icon, iconColor, label, value, unit }: StatTileProps) {
  return (
    <View style={styles.tile}>
      <Icon color={iconColor} size={20} />
      <Text style={styles.label}>{label}</Text>
      <Text style={styles.value}>
        {value}
        {unit ? <Text style={styles.unit}> {unit}</Text> : null}
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  tile: {
    flex: 1,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    gap: 6,
  },
  label: { color: theme.colors.textMuted, fontSize: 11, marginTop: 4 },
  value: { color: theme.colors.text, fontSize: 22, fontWeight: '700' },
  unit: { fontSize: 12, fontWeight: '500', color: theme.colors.textMuted },
});
