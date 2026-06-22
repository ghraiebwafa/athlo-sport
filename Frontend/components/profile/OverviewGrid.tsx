import type { ReactNode } from 'react';
import { LucideIcon } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface OverviewTileProps {
  icon: LucideIcon;
  iconColor: string;
  value: string;
  label: string;
}

export function OverviewTile({ icon: Icon, iconColor, value, label }: OverviewTileProps) {
  return (
    <View style={styles.tile}>
      <Icon color={iconColor} size={20} />
      <Text style={styles.value}>{value}</Text>
      <Text style={styles.label}>{label}</Text>
    </View>
  );
}

export function OverviewGrid({ children }: { children: ReactNode }) {
  return (
    <View style={styles.section}>
      <Text style={styles.sectionTitle}>Overview</Text>
      <View style={styles.grid}>{children}</View>
    </View>
  );
}

const styles = StyleSheet.create({
  section: { marginBottom: theme.spacing.lg },
  sectionTitle: { color: theme.colors.text, fontWeight: '700', fontSize: 16, marginBottom: theme.spacing.sm },
  grid: { flexDirection: 'row', flexWrap: 'wrap', gap: theme.spacing.sm },
  tile: {
    width: '48%',
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    gap: 6,
  },
  value: { color: theme.colors.text, fontWeight: '800', fontSize: 18 },
  label: { color: theme.colors.textMuted, fontSize: 12 },
});
