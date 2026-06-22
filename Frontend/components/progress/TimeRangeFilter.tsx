import { Pressable, ScrollView, StyleSheet, Text } from 'react-native';
import { theme } from '@/constants/theme';
import { TIME_RANGES, type TimeRange } from '@/lib/progressFilters';

interface TimeRangeFilterProps {
  value: TimeRange;
  onChange: (range: TimeRange) => void;
}

export function TimeRangeFilter({ value, onChange }: TimeRangeFilterProps) {
  return (
    <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.row}>
      {TIME_RANGES.map((range) => {
        const active = value === range;
        return (
          <Pressable
            key={range}
            onPress={() => onChange(range)}
            style={[styles.pill, active && styles.pillActive]}
          >
            <Text style={[styles.text, active && styles.textActive]}>{range}</Text>
          </Pressable>
        );
      })}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  row: { gap: 8, paddingVertical: 4 },
  pill: {
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: theme.radius.full,
    backgroundColor: theme.colors.surface,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  pillActive: { backgroundColor: theme.colors.primary, borderColor: theme.colors.primary },
  text: { color: theme.colors.textMuted, fontWeight: '600', fontSize: 13 },
  textActive: { color: '#fff' },
});
