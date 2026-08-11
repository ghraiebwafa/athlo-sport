import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { router } from 'expo-router';
import { Plus, Trash2 } from 'lucide-react-native';
import { useMemo, useState } from 'react';
import {
  Alert,
  FlatList,
  KeyboardAvoidingView,
  Modal,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { AdminGuard } from '@/components/auth/AdminGuard';
import { Button } from '@/components/ui/Button';
import { FormErrorBanner } from '@/components/ui/FormErrorBanner';
import { Input } from '@/components/ui/Input';
import { QueryState } from '@/components/ui/QueryState';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { createProgram, deleteProgram, getExercises } from '@/lib/api/admin';
import { getApiErrorMessage, parseApiError } from '@/lib/api/client';
import { getCategories, getPrograms } from '@/lib/api/programs';
import { ROUTES } from '@/lib/routes';
import type { WorkoutDifficulty } from '@/lib/types';

const DIFFICULTIES: WorkoutDifficulty[] = ['Beginner', 'Intermediate', 'Advanced'];

export default function AdminProgramsScreen() {
  const queryClient = useQueryClient();
  const [showCreate, setShowCreate] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [duration, setDuration] = useState('45');
  const [calories, setCalories] = useState('300');
  const [difficulty, setDifficulty] = useState<WorkoutDifficulty>('Beginner');
  const [categoryId, setCategoryId] = useState('');
  const [exerciseId, setExerciseId] = useState('');
  const [sets, setSets] = useState('3');
  const [reps, setReps] = useState('10');
  const [featured, setFeatured] = useState(false);
  const [error, setError] = useState('');

  const programsQuery = useQuery({ queryKey: ['programs'], queryFn: getPrograms });
  const categoriesQuery = useQuery({ queryKey: ['categories'], queryFn: getCategories });
  const exercisesQuery = useQuery({ queryKey: ['exercises'], queryFn: getExercises });

  const categories = categoriesQuery.data ?? [];
  const exercises = exercisesQuery.data ?? [];

  const selectedCategory = useMemo(
    () => categories.find((c) => c.id === categoryId) ?? categories[0],
    [categories, categoryId]
  );
  const selectedExercise = useMemo(
    () => exercises.find((e) => e.id === exerciseId) ?? exercises[0],
    [exercises, exerciseId]
  );

  const createMutation = useMutation({
    mutationFn: () => {
      const catId = selectedCategory?.id;
      const exId = selectedExercise?.id;
      if (!catId || !exId) throw new Error('Pick a category and at least one exercise.');
      return createProgram({
        name: name.trim(),
        description: description.trim(),
        durationMinutes: Number(duration) || 45,
        estimatedCalories: Number(calories) || 300,
        difficulty,
        isFeatured: featured,
        categoryId: catId,
        exercises: [
          {
            exerciseId: exId,
            orderIndex: 0,
            sets: Number(sets) || 3,
            reps: Number(reps) || 10,
          },
        ],
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['programs'] });
      closeCreate();
    },
    onError: (err) => setError(parseApiError(err).message),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteProgram,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['programs'] });
    },
    onError: (err) => Alert.alert('Error', getApiErrorMessage(err)),
  });

  const openCreate = () => {
    setName('');
    setDescription('');
    setDuration('45');
    setCalories('300');
    setDifficulty('Beginner');
    setCategoryId(categories[0]?.id ?? '');
    setExerciseId(exercises[0]?.id ?? '');
    setSets('3');
    setReps('10');
    setFeatured(false);
    setError('');
    setShowCreate(true);
  };

  const closeCreate = () => {
    setShowCreate(false);
    setError('');
  };

  return (
    <AdminGuard>
      <View style={styles.screen}>
        <ScreenHeader
          title="Programs"
          onBack={() => router.back()}
          right={
            <Pressable onPress={openCreate} accessibilityRole="button" accessibilityLabel="Add program">
              <Plus color={theme.colors.primary} size={24} />
            </Pressable>
          }
        />

        {programsQuery.isError ? (
          <QueryState
            message={getApiErrorMessage(programsQuery.error)}
            onRetry={() => void programsQuery.refetch()}
          />
        ) : (
          <FlatList
            data={programsQuery.data ?? []}
            keyExtractor={(item) => item.id}
            contentContainerStyle={styles.list}
            refreshing={programsQuery.isFetching}
            onRefresh={() => void programsQuery.refetch()}
            ListEmptyComponent={
              !programsQuery.isLoading ? <Text style={styles.empty}>No programs yet.</Text> : null
            }
            renderItem={({ item }) => (
              <View style={styles.row}>
                <View style={styles.rowBody}>
                  <Text style={styles.name}>{item.name}</Text>
                  <Text style={styles.meta}>
                    {item.categoryName} · {item.difficulty} · {item.durationMinutes} min ·{' '}
                    {item.exerciseCount} exercises
                    {item.isFeatured ? ' · Featured' : ''}
                  </Text>
                </View>
                <Pressable
                  onPress={() =>
                    Alert.alert('Delete program?', item.name, [
                      { text: 'Cancel', style: 'cancel' },
                      {
                        text: 'Delete',
                        style: 'destructive',
                        onPress: () => deleteMutation.mutate(item.id),
                      },
                    ])
                  }
                  hitSlop={8}
                  accessibilityLabel={`Delete ${item.name}`}
                >
                  <Trash2 color={theme.colors.red} size={18} />
                </Pressable>
              </View>
            )}
          />
        )}

        <Modal visible={showCreate} animationType="slide" transparent onRequestClose={closeCreate}>
          <KeyboardAvoidingView
            behavior={Platform.OS === 'ios' ? 'padding' : undefined}
            style={styles.modalWrap}
          >
            <ScrollView contentContainerStyle={styles.modalCard} keyboardShouldPersistTaps="handled">
              <Text style={styles.modalTitle}>New program</Text>
              <Input label="Name" value={name} onChangeText={setName} />
              <Input label="Description" value={description} onChangeText={setDescription} />
              <View style={styles.rowInputs}>
                <View style={styles.half}>
                  <Input label="Duration (min)" value={duration} onChangeText={setDuration} keyboardType="number-pad" />
                </View>
                <View style={styles.half}>
                  <Input label="Calories" value={calories} onChangeText={setCalories} keyboardType="number-pad" />
                </View>
              </View>

              <Text style={styles.label}>Difficulty</Text>
              <View style={styles.chips}>
                {DIFFICULTIES.map((d) => (
                  <Pressable
                    key={d}
                    style={[styles.chip, difficulty === d && styles.chipActive]}
                    onPress={() => setDifficulty(d)}
                  >
                    <Text style={[styles.chipText, difficulty === d && styles.chipTextActive]}>{d}</Text>
                  </Pressable>
                ))}
              </View>

              <Text style={styles.label}>Category</Text>
              <View style={styles.chips}>
                {categories.map((c) => (
                  <Pressable
                    key={c.id}
                    style={[styles.chip, (categoryId || categories[0]?.id) === c.id && styles.chipActive]}
                    onPress={() => setCategoryId(c.id)}
                  >
                    <Text
                      style={[
                        styles.chipText,
                        (categoryId || categories[0]?.id) === c.id && styles.chipTextActive,
                      ]}
                    >
                      {c.name}
                    </Text>
                  </Pressable>
                ))}
              </View>

              <Text style={styles.label}>First exercise</Text>
              <View style={styles.chips}>
                {exercises.slice(0, 12).map((e) => (
                  <Pressable
                    key={e.id}
                    style={[styles.chip, (exerciseId || exercises[0]?.id) === e.id && styles.chipActive]}
                    onPress={() => setExerciseId(e.id)}
                  >
                    <Text
                      style={[
                        styles.chipText,
                        (exerciseId || exercises[0]?.id) === e.id && styles.chipTextActive,
                      ]}
                    >
                      {e.name}
                    </Text>
                  </Pressable>
                ))}
              </View>

              <View style={styles.rowInputs}>
                <View style={styles.half}>
                  <Input label="Sets" value={sets} onChangeText={setSets} keyboardType="number-pad" />
                </View>
                <View style={styles.half}>
                  <Input label="Reps" value={reps} onChangeText={setReps} keyboardType="number-pad" />
                </View>
              </View>

              <Pressable
                style={styles.featuredRow}
                onPress={() => setFeatured((v) => !v)}
                accessibilityRole="checkbox"
                accessibilityState={{ checked: featured }}
              >
                <View style={[styles.checkbox, featured && styles.checkboxOn]} />
                <Text style={styles.featuredLabel}>Featured on home</Text>
              </Pressable>

              <FormErrorBanner message={error} />
              <Button
                title="Create program"
                onPress={() => {
                  if (!name.trim() || !description.trim()) {
                    setError('Name and description are required.');
                    return;
                  }
                  if (!selectedCategory || !selectedExercise) {
                    setError('Add a category and exercise first.');
                    return;
                  }
                  setError('');
                  createMutation.mutate();
                }}
                loading={createMutation.isPending}
              />
              <Button title="Cancel" variant="secondary" onPress={closeCreate} />
              <Button title="Manage exercises" variant="ghost" onPress={() => router.push(ROUTES.adminExercises)} />
            </ScrollView>
          </KeyboardAvoidingView>
        </Modal>
      </View>
    </AdminGuard>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  list: { padding: theme.spacing.md, paddingBottom: 40, gap: 8 },
  empty: { color: theme.colors.textMuted, textAlign: 'center', marginTop: 40 },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
  },
  rowBody: { flex: 1 },
  name: { color: theme.colors.text, fontWeight: '600', fontSize: 16 },
  meta: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
  modalWrap: {
    flex: 1,
    justifyContent: 'flex-end',
    backgroundColor: 'rgba(0,0,0,0.55)',
  },
  modalCard: {
    backgroundColor: theme.colors.surface,
    borderTopLeftRadius: theme.radius.xl,
    borderTopRightRadius: theme.radius.xl,
    padding: theme.spacing.lg,
    gap: theme.spacing.sm,
    paddingBottom: 40,
  },
  modalTitle: { color: theme.colors.text, fontWeight: '700', fontSize: 18, marginBottom: 4 },
  rowInputs: { flexDirection: 'row', gap: theme.spacing.sm },
  half: { flex: 1 },
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
});
