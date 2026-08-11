import { Check } from 'lucide-react-native';
import { useState } from 'react';
import { Pressable, StyleSheet, Text, TextInput, View } from 'react-native';
import { theme } from '@/constants/theme';
import type { ProgramExercise, WorkoutSetLog } from '@/lib/types';

interface SetLoggerProps {
  exercise: ProgramExercise;
  loggedSets: WorkoutSetLog[];
  busy?: boolean;
  onLogSet: (input: {
    programExerciseId: string;
    setNumber: number;
    repsCompleted: number;
    weightKg?: number;
  }) => void;
}

export function SetLogger({ exercise, loggedSets, busy, onLogSet }: SetLoggerProps) {
  const completedNumbers = new Set(
    loggedSets.filter((s) => s.completed).map((s) => s.setNumber)
  );
  const nextSet =
    Array.from({ length: exercise.sets }, (_, i) => i + 1).find((n) => !completedNumbers.has(n)) ??
    exercise.sets;

  const lastLog = [...loggedSets].sort((a, b) => b.setNumber - a.setNumber)[0];
  const [reps, setReps] = useState(String(lastLog?.repsCompleted ?? exercise.reps));
  const [weight, setWeight] = useState(
    lastLog?.weightKg != null ? String(lastLog.weightKg) : ''
  );

  const allDone = completedNumbers.size >= exercise.sets;

  const handleLog = () => {
    const repsCompleted = Math.max(0, parseInt(reps, 10) || 0);
    const parsedWeight = weight.trim() === '' ? undefined : Number(weight);
    onLogSet({
      programExerciseId: exercise.id,
      setNumber: nextSet,
      repsCompleted,
      weightKg:
        parsedWeight != null && Number.isFinite(parsedWeight) && parsedWeight >= 0
          ? parsedWeight
          : undefined,
    });
  };

  return (
    <View style={styles.card}>
      <Text style={styles.title}>Log sets</Text>
      <View style={styles.setRow}>
        {Array.from({ length: exercise.sets }, (_, i) => i + 1).map((n) => {
          const done = completedNumbers.has(n);
          return (
            <View key={n} style={[styles.chip, done && styles.chipDone]}>
              {done ? <Check color={theme.colors.primary} size={14} /> : null}
              <Text style={[styles.chipText, done && styles.chipTextDone]}>Set {n}</Text>
            </View>
          );
        })}
      </View>

      {allDone ? (
        <Text style={styles.done}>All sets logged for this exercise.</Text>
      ) : (
        <>
          <Text style={styles.subtitle}>
            Set {nextSet} of {exercise.sets}
          </Text>
          <View style={styles.inputs}>
            <View style={styles.field}>
              <Text style={styles.label}>Reps</Text>
              <TextInput
                style={styles.input}
                keyboardType="number-pad"
                value={reps}
                onChangeText={setReps}
                placeholder={String(exercise.reps)}
                placeholderTextColor={theme.colors.textMuted}
                accessibilityLabel="Reps completed"
              />
            </View>
            <View style={styles.field}>
              <Text style={styles.label}>Weight (kg)</Text>
              <TextInput
                style={styles.input}
                keyboardType="decimal-pad"
                value={weight}
                onChangeText={setWeight}
                placeholder="Optional"
                placeholderTextColor={theme.colors.textMuted}
                accessibilityLabel="Weight in kilograms"
              />
            </View>
          </View>
          <Pressable
            style={[styles.button, busy && styles.buttonDisabled]}
            onPress={handleLog}
            disabled={busy}
            accessibilityRole="button"
            accessibilityLabel={`Log set ${nextSet}`}
          >
            <Text style={styles.buttonText}>{busy ? 'Saving…' : `Complete set ${nextSet}`}</Text>
          </Pressable>
        </>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.xl,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginBottom: theme.spacing.md,
    gap: theme.spacing.sm,
  },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  subtitle: { color: theme.colors.primary, fontWeight: '600', fontSize: 13 },
  setRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 8 },
  chip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.surfaceLight,
  },
  chipDone: { backgroundColor: `${theme.colors.primary}22` },
  chipText: { color: theme.colors.textMuted, fontSize: 12, fontWeight: '600' },
  chipTextDone: { color: theme.colors.primary },
  inputs: { flexDirection: 'row', gap: theme.spacing.sm },
  field: { flex: 1, gap: 4 },
  label: { color: theme.colors.textMuted, fontSize: 12, fontWeight: '500' },
  input: {
    backgroundColor: theme.colors.background,
    borderWidth: 1,
    borderColor: theme.colors.border,
    borderRadius: theme.radius.md,
    paddingHorizontal: 12,
    paddingVertical: 10,
    color: theme.colors.text,
    fontSize: 16,
  },
  button: {
    backgroundColor: theme.colors.primary,
    borderRadius: theme.radius.lg,
    paddingVertical: 12,
    alignItems: 'center',
  },
  buttonDisabled: { opacity: 0.6 },
  buttonText: { color: '#fff', fontWeight: '700', fontSize: 15 },
  done: { color: theme.colors.textMuted, fontSize: 13 },
});
