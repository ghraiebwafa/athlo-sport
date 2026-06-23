import { router } from 'expo-router';
import { ChevronRight, Clock, Dumbbell } from 'lucide-react-native';
import { Image, Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import { programDetail } from '@/lib/routes';
import type { ProgramListItem } from '@/lib/types';

interface PopularWorkoutRowProps {
  program: ProgramListItem;
}

export function PopularWorkoutRow({ program }: PopularWorkoutRowProps) {
  return (
    <Pressable
      style={({ pressed }) => [styles.row, pressed && styles.pressed]}
      onPress={() => router.push(programDetail(program.id))}
    >
      {program.imageUrl ? (
        <Image source={{ uri: program.imageUrl }} style={styles.thumb} />
      ) : (
        <View style={styles.thumbPlaceholder}>
          <Dumbbell color={theme.colors.primary} size={18} />
        </View>
      )}
      <View style={styles.text}>
        <Text style={styles.title} numberOfLines={1}>{program.name}</Text>
        <View style={styles.meta}>
          <Clock color={theme.colors.textMuted} size={12} />
          <Text style={styles.metaText}>{program.durationMinutes} min</Text>
        </View>
      </View>
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
    padding: theme.spacing.sm,
    marginBottom: theme.spacing.sm,
    borderWidth: 1,
    borderColor: theme.colors.border,
    gap: theme.spacing.sm,
  },
  pressed: { opacity: 0.9 },
  thumb: { width: 56, height: 56, borderRadius: theme.radius.md },
  thumbPlaceholder: {
    width: 56,
    height: 56,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.surfaceLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  text: { flex: 1 },
  title: { color: theme.colors.text, fontWeight: '600', fontSize: 15 },
  meta: { flexDirection: 'row', alignItems: 'center', gap: 4, marginTop: 4 },
  metaText: { color: theme.colors.textMuted, fontSize: 12 },
});
