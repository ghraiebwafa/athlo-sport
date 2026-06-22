import { ChevronRight, Dumbbell } from 'lucide-react-native';
import { Image, Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import type { ProgramExercise } from '@/lib/types';

interface ExerciseListItemProps {
  exercise: ProgramExercise;
}

export function ExerciseListItem({ exercise }: ExerciseListItemProps) {
  const meta = exercise.durationSeconds
    ? `${exercise.sets} sets × ${exercise.durationSeconds} sec`
    : `${exercise.sets} sets × ${exercise.reps} reps`;

  return (
    <Pressable style={({ pressed }) => [styles.row, pressed && styles.pressed]}>
      {exercise.imageUrl ? (
        <Image source={{ uri: exercise.imageUrl }} style={styles.thumb} />
      ) : (
        <View style={styles.thumbPlaceholder}>
          <Dumbbell color={theme.colors.primary} size={18} />
        </View>
      )}
      <View style={styles.badge}>
        <Text style={styles.badgeText}>{exercise.orderIndex}</Text>
      </View>
      <View style={styles.text}>
        <Text style={styles.name}>{exercise.name}</Text>
        <Text style={styles.meta}>{meta}</Text>
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
  thumb: { width: 52, height: 52, borderRadius: theme.radius.md },
  thumbPlaceholder: {
    width: 52,
    height: 52,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.surfaceLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  badge: {
    position: 'absolute',
    left: 44,
    top: 8,
    width: 22,
    height: 22,
    borderRadius: 11,
    backgroundColor: theme.colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  badgeText: { color: '#fff', fontSize: 11, fontWeight: '700' },
  text: { flex: 1, marginLeft: 8 },
  name: { color: theme.colors.text, fontWeight: '600', fontSize: 15 },
  meta: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
});
