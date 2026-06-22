import { router } from 'expo-router';
import { Calendar, ChevronRight } from 'lucide-react-native';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import type { ProgramListItem } from '@/lib/types';

interface UpcomingWorkoutRowProps {
  program: ProgramListItem;
}

export function UpcomingWorkoutRow({ program }: UpcomingWorkoutRowProps) {
  return (
    <Pressable
      style={({ pressed }) => [styles.row, pressed && styles.pressed]}
      onPress={() => router.push(`/program/${program.id}`)}
    >
      <View style={styles.iconWrap}>
        <Calendar color={theme.colors.primary} size={20} />
      </View>
      <View style={styles.text}>
        <Text style={styles.title}>{program.name}</Text>
        <Text style={styles.subtitle}>Suggested next</Text>
      </View>
      <Text style={styles.time}>{program.durationMinutes} min</Text>
      <ChevronRight color={theme.colors.textMuted} size={20} />
    </Pressable>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    gap: theme.spacing.sm,
  },
  pressed: { opacity: 0.9 },
  iconWrap: {
    width: 40,
    height: 40,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.surfaceLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  text: { flex: 1 },
  title: { color: theme.colors.text, fontWeight: '600', fontSize: 15 },
  subtitle: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
  time: { color: theme.colors.primary, fontWeight: '600', fontSize: 13 },
});
