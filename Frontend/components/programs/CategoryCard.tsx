import { Pressable, StyleSheet, Text } from 'react-native';
import { getCategoryIcon } from '@/lib/categoryIcons';
import { theme } from '@/constants/theme';
import type { Category } from '@/lib/types';

interface CategoryCardProps {
  category: Category;
  selected?: boolean;
  onPress: () => void;
}

export function CategoryCard({ category, selected, onPress }: CategoryCardProps) {
  const { Icon, color } = getCategoryIcon(category.icon, category.slug);

  return (
    <Pressable
      onPress={onPress}
      style={[styles.card, selected && styles.cardSelected]}
    >
      <Icon color={color} size={22} />
      <Text style={styles.name}>{category.name}</Text>
      <Text style={styles.count}>{category.programCount} Programs</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    width: 100,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.sm,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: theme.colors.border,
    gap: 6,
  },
  cardSelected: { borderColor: theme.colors.primary },
  name: { color: theme.colors.text, fontWeight: '600', fontSize: 13, textAlign: 'center' },
  count: { color: theme.colors.textMuted, fontSize: 10, textAlign: 'center' },
});
