import { ChevronDown, ChevronUp, Plus, Trash2 } from 'lucide-react-native';
import { useState } from 'react';
import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { Button } from '@/components/ui/Button';
import { FormErrorBanner } from '@/components/ui/FormErrorBanner';
import { Input } from '@/components/ui/Input';
import { theme } from '@/constants/theme';
import type {
  Category,
  CreateProgramPayload,
  Exercise,
  ProgramExerciseInput,
  WorkoutDifficulty,
} from '@/lib/types';

const DIFFICULTIES: WorkoutDifficulty[] = ['Beginner', 'Intermediate', 'Advanced'];

export interface ProgramFormExerciseRow {
  key: string;
  exerciseId: string;
  sets: string;
  reps: string;
  durationSeconds: string;
}

export interface ProgramFormValues {
  name: string;
  description: string;
  durationMinutes: string;
  estimatedCalories: string;
  difficulty: WorkoutDifficulty;
  categoryId: string;
  isFeatured: boolean;
  imageUrl: string;
  exercises: ProgramFormExerciseRow[];
}

interface ProgramFormProps {
  categories: Category[];
  exercises: Exercise[];
  initial: ProgramFormValues;
  submitLabel: string;
  loading?: boolean;
  error?: string;
  onSubmit: (payload: CreateProgramPayload) => void;
  onCancel: () => void;
}

export function emptyExerciseRow(exerciseId = ''): ProgramFormExerciseRow {
  return {
    key: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
    exerciseId,
    sets: '3',
    reps: '10',
    durationSeconds: '',
  };
}

export function buildProgramPayload(
  values: ProgramFormValues
): { ok: true; payload: CreateProgramPayload } | { ok: false; message: string } {
  if (!values.name.trim() || !values.description.trim()) {
    return { ok: false, message: 'Name and description are required.' };
  }
  if (!values.categoryId) {
    return { ok: false, message: 'Select a category.' };
  }
  if (values.exercises.length === 0) {
    return { ok: false, message: 'Add at least one exercise.' };
  }

  const exercises: ProgramExerciseInput[] = [];
  for (let i = 0; i < values.exercises.length; i++) {
    const row = values.exercises[i];
    if (!row.exerciseId) {
      return { ok: false, message: `Pick an exercise for row ${i + 1}.` };
    }
    const sets = Number(row.sets);
    const reps = Number(row.reps);
    if (!Number.isFinite(sets) || sets < 1 || sets > 20) {
      return { ok: false, message: `Sets for row ${i + 1} must be between 1 and 20.` };
    }
    if (!Number.isFinite(reps) || reps < 0 || reps > 500) {
      return { ok: false, message: `Reps for row ${i + 1} must be between 0 and 500.` };
    }
    const durationRaw = row.durationSeconds.trim();
    const durationSeconds = durationRaw === '' ? undefined : Number(durationRaw);
    if (durationSeconds != null && (!Number.isFinite(durationSeconds) || durationSeconds < 1)) {
      return { ok: false, message: `Duration for row ${i + 1} must be at least 1 second.` };
    }

    exercises.push({
      exerciseId: row.exerciseId,
      orderIndex: i + 1,
      sets,
      reps,
      durationSeconds,
    });
  }

  return {
    ok: true,
    payload: {
      name: values.name.trim(),
      description: values.description.trim(),
      durationMinutes: Number(values.durationMinutes) || 45,
      estimatedCalories: Number(values.estimatedCalories) || 300,
      difficulty: values.difficulty,
      isFeatured: values.isFeatured,
      categoryId: values.categoryId,
      imageUrl: values.imageUrl.trim() || undefined,
      exercises,
    },
  };
}

export function ProgramForm({
  categories,
  exercises,
  initial,
  submitLabel,
  loading,
  error,
  onSubmit,
  onCancel,
}: ProgramFormProps) {
  const [values, setValues] = useState<ProgramFormValues>(initial);
  const [localError, setLocalError] = useState('');
  const [searchByRow, setSearchByRow] = useState<Record<string, string>>({});

  const update = (patch: Partial<ProgramFormValues>) => setValues((v) => ({ ...v, ...patch }));

  const updateRow = (key: string, patch: Partial<ProgramFormExerciseRow>) => {
    setValues((v) => ({
      ...v,
      exercises: v.exercises.map((row) => (row.key === key ? { ...row, ...patch } : row)),
    }));
  };

  const moveRow = (index: number, direction: -1 | 1) => {
    const next = index + direction;
    if (next < 0 || next >= values.exercises.length) return;
    setValues((v) => {
      const copy = [...v.exercises];
      const [item] = copy.splice(index, 1);
      copy.splice(next, 0, item);
      return { ...v, exercises: copy };
    });
  };

  const handleSubmit = () => {
    setLocalError('');
    const result = buildProgramPayload(values);
    if (!result.ok) {
      setLocalError(result.message);
      return;
    }
    onSubmit(result.payload);
  };

  return (
    <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
      <Input label="Name" value={values.name} onChangeText={(name) => update({ name })} />
      <Input
        label="Description"
        value={values.description}
        onChangeText={(description) => update({ description })}
      />
      <Input
        label="Image URL (optional)"
        value={values.imageUrl}
        onChangeText={(imageUrl) => update({ imageUrl })}
        autoCapitalize="none"
      />

      <View style={styles.rowInputs}>
        <View style={styles.half}>
          <Input
            label="Duration (min)"
            value={values.durationMinutes}
            onChangeText={(durationMinutes) => update({ durationMinutes })}
            keyboardType="number-pad"
          />
        </View>
        <View style={styles.half}>
          <Input
            label="Calories"
            value={values.estimatedCalories}
            onChangeText={(estimatedCalories) => update({ estimatedCalories })}
            keyboardType="number-pad"
          />
        </View>
      </View>

      <Text style={styles.label}>Difficulty</Text>
      <View style={styles.chips}>
        {DIFFICULTIES.map((d) => (
          <Pressable
            key={d}
            style={[styles.chip, values.difficulty === d && styles.chipActive]}
            onPress={() => update({ difficulty: d })}
          >
            <Text style={[styles.chipText, values.difficulty === d && styles.chipTextActive]}>{d}</Text>
          </Pressable>
        ))}
      </View>

      <Text style={styles.label}>Category</Text>
      <View style={styles.chips}>
        {categories.map((c) => (
          <Pressable
            key={c.id}
            style={[styles.chip, values.categoryId === c.id && styles.chipActive]}
            onPress={() => update({ categoryId: c.id })}
          >
            <Text style={[styles.chipText, values.categoryId === c.id && styles.chipTextActive]}>
              {c.name}
            </Text>
          </Pressable>
        ))}
      </View>

      <Pressable
        style={styles.featuredRow}
        onPress={() => update({ isFeatured: !values.isFeatured })}
        accessibilityRole="checkbox"
        accessibilityState={{ checked: values.isFeatured }}
      >
        <View style={[styles.checkbox, values.isFeatured && styles.checkboxOn]} />
        <Text style={styles.featuredLabel}>Featured on home</Text>
      </Pressable>

      <View style={styles.sectionHeader}>
        <Text style={styles.sectionTitle}>Exercises</Text>
        <Pressable
          onPress={() =>
            update({
              exercises: [...values.exercises, emptyExerciseRow(exercises[0]?.id ?? '')],
            })
          }
          accessibilityRole="button"
          accessibilityLabel="Add exercise"
          style={styles.addBtn}
        >
          <Plus color={theme.colors.primary} size={18} />
          <Text style={styles.addText}>Add</Text>
        </Pressable>
      </View>

      {values.exercises.map((row, index) => (
        <View key={row.key} style={styles.exerciseCard}>
          <View style={styles.exerciseTop}>
            <Text style={styles.exerciseIndex}>#{index + 1}</Text>
            <View style={styles.exerciseActions}>
              <Pressable onPress={() => moveRow(index, -1)} hitSlop={6} accessibilityLabel="Move up">
                <ChevronUp color={theme.colors.textMuted} size={18} />
              </Pressable>
              <Pressable onPress={() => moveRow(index, 1)} hitSlop={6} accessibilityLabel="Move down">
                <ChevronDown color={theme.colors.textMuted} size={18} />
              </Pressable>
              <Pressable
                onPress={() =>
                  update({ exercises: values.exercises.filter((e) => e.key !== row.key) })
                }
                hitSlop={6}
                accessibilityLabel="Remove exercise"
              >
                <Trash2 color={theme.colors.red} size={16} />
              </Pressable>
            </View>
          </View>

          <Text style={styles.label}>Exercise</Text>
          {row.exerciseId ? (
            <Text style={styles.selectedExercise}>
              Selected: {exercises.find((e) => e.id === row.exerciseId)?.name ?? 'Unknown'}
            </Text>
          ) : null}
          <Input
            label="Search exercises"
            value={searchByRow[row.key] ?? ''}
            onChangeText={(text) => setSearchByRow((prev) => ({ ...prev, [row.key]: text }))}
            placeholder="Type to filter…"
            autoCapitalize="none"
          />
          <View style={styles.chips}>
            {exercises
              .filter((e) => {
                const q = (searchByRow[row.key] ?? '').trim().toLowerCase();
                if (!q) return true;
                return e.name.toLowerCase().includes(q);
              })
              .slice(0, 24)
              .map((e) => (
                <Pressable
                  key={e.id}
                  style={[styles.chip, row.exerciseId === e.id && styles.chipActive]}
                  onPress={() => {
                    updateRow(row.key, { exerciseId: e.id });
                    setSearchByRow((prev) => ({ ...prev, [row.key]: '' }));
                  }}
                >
                  <Text style={[styles.chipText, row.exerciseId === e.id && styles.chipTextActive]}>
                    {e.name}
                  </Text>
                </Pressable>
              ))}
          </View>
          {(searchByRow[row.key] ?? '').trim() &&
          exercises.filter((e) => e.name.toLowerCase().includes((searchByRow[row.key] ?? '').trim().toLowerCase()))
            .length === 0 ? (
            <Text style={styles.emptySearch}>No exercises match that search.</Text>
          ) : null}

          <View style={styles.rowInputs}>
            <View style={styles.third}>
              <Input
                label="Sets"
                value={row.sets}
                onChangeText={(sets) => updateRow(row.key, { sets })}
                keyboardType="number-pad"
              />
            </View>
            <View style={styles.third}>
              <Input
                label="Reps"
                value={row.reps}
                onChangeText={(reps) => updateRow(row.key, { reps })}
                keyboardType="number-pad"
              />
            </View>
            <View style={styles.third}>
              <Input
                label="Sec (opt)"
                value={row.durationSeconds}
                onChangeText={(durationSeconds) => updateRow(row.key, { durationSeconds })}
                keyboardType="number-pad"
              />
            </View>
          </View>
        </View>
      ))}

      <FormErrorBanner message={localError || error || ''} />
      <Button title={submitLabel} onPress={handleSubmit} loading={loading} />
      <Button title="Cancel" variant="secondary" onPress={onCancel} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  content: { padding: theme.spacing.md, paddingBottom: 48, gap: theme.spacing.sm },
  rowInputs: { flexDirection: 'row', gap: theme.spacing.sm },
  half: { flex: 1 },
  third: { flex: 1 },
  label: { color: theme.colors.textMuted, fontSize: 14, fontWeight: '500', marginTop: 4 },
  chips: { flexDirection: 'row', flexWrap: 'wrap', gap: 8 },
  chip: {
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.surfaceLight,
  },
  chipActive: { backgroundColor: `${theme.colors.primary}33` },
  chipText: { color: theme.colors.textMuted, fontSize: 13, fontWeight: '600' },
  chipTextActive: { color: theme.colors.primary },
  featuredRow: { flexDirection: 'row', alignItems: 'center', gap: 10, marginVertical: 4 },
  checkbox: {
    width: 20,
    height: 20,
    borderRadius: 4,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  checkboxOn: { backgroundColor: theme.colors.primary, borderColor: theme.colors.primary },
  featuredLabel: { color: theme.colors.text },
  sectionHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: theme.spacing.sm,
  },
  sectionTitle: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  addBtn: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  addText: { color: theme.colors.primary, fontWeight: '600' },
  exerciseCard: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
    gap: theme.spacing.sm,
  },
  exerciseTop: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  exerciseIndex: { color: theme.colors.primary, fontWeight: '700' },
  exerciseActions: { flexDirection: 'row', alignItems: 'center', gap: 10 },
  selectedExercise: { color: theme.colors.text, fontSize: 13, fontWeight: '600' },
  emptySearch: { color: theme.colors.textMuted, fontSize: 12 },
});
