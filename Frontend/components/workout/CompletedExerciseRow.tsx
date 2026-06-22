import { CheckCircle2, Dumbbell } from 'lucide-react-native';
import { Image, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import type { ProgramExercise } from '@/lib/types';

interface CompletedExerciseRowProps {
  exercise: ProgramExercise;
}

export function CompletedExerciseRow({ exercise }: CompletedExerciseRowProps) {
  const meta = exercise.durationSeconds
    ? `${exercise.sets} sets × ${exercise.durationSeconds} sec`
    : `${exercise.sets} sets × ${exercise.reps} reps`;

  return (
    <View style={styles.row}>
      {exercise.imageUrl ? (
        <Image source={{ uri: exercise.imageUrl }} style={styles.thumb} />
      ) : (
        <View style={styles.thumbPlaceholder}>
          <Dumbbell color={theme.colors.primary} size={16} />
        </View>
      )}
      <View style={styles.text}>
        <Text style={styles.name}>
          {exercise.orderIndex}. {exercise.name}
        </Text>
        <Text style={styles.meta}>{meta}</Text>
      </View>
      <CheckCircle2 color={theme.colors.success} size={22} />
    </View>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.sm,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.sm,
    marginBottom: theme.spacing.sm,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  thumb: { width: 48, height: 48, borderRadius: theme.radius.sm },
  thumbPlaceholder: {
    width: 48,
    height: 48,
    borderRadius: theme.radius.sm,
    backgroundColor: theme.colors.surfaceLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  text: { flex: 1 },
  name: { color: theme.colors.text, fontWeight: '600', fontSize: 14 },
  meta: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
});
