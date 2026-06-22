import { Activity, Dumbbell, Flame, Flower2, LucideIcon } from 'lucide-react-native';
import { theme } from '@/constants/theme';

const iconMap: Record<string, LucideIcon> = {
  dumbbell: Dumbbell,
  flame: Flame,
  running: Activity,
  cardio: Activity,
  lotus: Flower2,
  yoga: Flower2,
};

const colorMap: Record<string, string> = {
  strength: theme.colors.primary,
  hiit: theme.colors.red,
  cardio: theme.colors.green,
  yoga: theme.colors.purple,
};

export function getCategoryIcon(icon: string, slug: string): { Icon: LucideIcon; color: string } {
  const Icon = iconMap[icon.toLowerCase()] ?? Dumbbell;
  const color = colorMap[slug.toLowerCase()] ?? theme.colors.primary;
  return { Icon, color };
}
