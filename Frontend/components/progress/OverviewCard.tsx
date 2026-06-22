import { LucideIcon, TrendingUp } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface OverviewCardProps {
  icon: LucideIcon;
  iconColor: string;
  value: string;
  label: string;
  trend?: number | null;
}

export function OverviewCard({ icon: Icon, iconColor, value, label, trend }: OverviewCardProps) {
  return (
    <View style={styles.card}>
      <Icon color={iconColor} size={18} />
      <Text style={styles.value}>{value}</Text>
      <Text style={styles.label}>{label}</Text>
      {trend != null ? (
        <View style={styles.trendRow}>
          <TrendingUp color={theme.colors.green} size={12} />
          <Text style={styles.trend}>↗ {Math.abs(trend)}% vs last period</Text>
        </View>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    width: 160,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginRight: theme.spacing.sm,
    gap: 4,
  },
  value: { color: theme.colors.text, fontSize: 20, fontWeight: '800', marginTop: 4 },
  label: { color: theme.colors.textMuted, fontSize: 12 },
  trendRow: { flexDirection: 'row', alignItems: 'center', gap: 4, marginTop: 4 },
  trend: { color: theme.colors.green, fontSize: 10 },
});
