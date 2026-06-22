import { Dumbbell } from 'lucide-react-native';
import { Image, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import type { ProgramExercise } from '@/lib/types';

interface UpNextCardProps {
  exercise: ProgramExercise;
}

export function UpNextCard({ exercise }: UpNextCardProps) {
  const meta = exercise.durationSeconds
    ? `${exercise.sets} sets × ${exercise.durationSeconds} sec`
    : `${exercise.sets} sets × ${exercise.reps} reps`;

  return (
    <View style={styles.wrap}>
      <Text style={styles.label}>Up Next</Text>
      <View style={styles.card}>
        {exercise.imageUrl ? (
          <Image source={{ uri: exercise.imageUrl }} style={styles.thumb} />
        ) : (
          <View style={styles.thumbPlaceholder}>
            <Dumbbell color={theme.colors.primary} size={16} />
          </View>
        )}
        <View>
          <Text style={styles.name}>{exercise.name}</Text>
          <Text style={styles.meta}>{meta}</Text>
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: { marginBottom: theme.spacing.md },
  label: { color: theme.colors.textMuted, fontSize: 13, marginBottom: theme.spacing.sm },
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.sm,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.sm,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  thumb: { width: 44, height: 44, borderRadius: theme.radius.sm },
  thumbPlaceholder: {
    width: 44,
    height: 44,
    borderRadius: theme.radius.sm,
    backgroundColor: theme.colors.surfaceLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  name: { color: theme.colors.text, fontWeight: '600', fontSize: 14 },
  meta: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
});
