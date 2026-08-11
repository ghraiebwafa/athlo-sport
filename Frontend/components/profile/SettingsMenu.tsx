import { ChevronRight, LucideIcon } from 'lucide-react-native';
import { Alert, Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface MenuItem {
  id: string;
  label: string;
  icon: LucideIcon;
  destructive?: boolean;
}

interface SettingsMenuProps {
  items: MenuItem[];
  onSelect: (id: string) => void;
}

export function SettingsMenu({ items, onSelect }: SettingsMenuProps) {
  return (
    <View style={styles.wrap}>
      {items.map((item) => {
        const Icon = item.icon;
        const color = item.destructive ? theme.colors.red : theme.colors.primary;
        return (
          <Pressable
            key={item.id}
            style={({ pressed }) => [styles.row, pressed && styles.pressed]}
            onPress={() => onSelect(item.id)}
            accessibilityRole="button"
            accessibilityLabel={item.label}
          >
            <Icon color={color} size={20} />
            <Text style={[styles.label, item.destructive && styles.destructive]}>{item.label}</Text>
            {!item.destructive ? <ChevronRight color={theme.colors.textMuted} size={20} /> : null}
          </Pressable>
        );
      })}
    </View>
  );
}

export function showComingSoon(feature: string) {
  Alert.alert('Coming soon', `${feature} will be available in a future update.`);
}

const styles = StyleSheet.create({
  wrap: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    overflow: 'hidden',
    marginBottom: theme.spacing.lg,
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    padding: theme.spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.border,
  },
  pressed: { opacity: 0.9 },
  label: { flex: 1, color: theme.colors.text, fontSize: 15, fontWeight: '500' },
  destructive: { color: theme.colors.red },
});
