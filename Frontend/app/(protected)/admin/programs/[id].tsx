import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { router, useLocalSearchParams } from 'expo-router';
import { useMemo, useState } from 'react';
import { ActivityIndicator, StyleSheet, View } from 'react-native';
import { AdminGuard } from '@/components/auth/AdminGuard';
import {
  ProgramForm,
  emptyExerciseRow,
  type ProgramFormValues,
} from '@/components/admin/ProgramForm';
import { QueryState } from '@/components/ui/QueryState';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { getExercises, updateProgram } from '@/lib/api/admin';
import { getApiErrorMessage, parseApiError } from '@/lib/api/client';
import { getCategories, getProgram } from '@/lib/api/programs';
import { ROUTES } from '@/lib/routes';
import { normalizeRouteParam } from '@/lib/routeParams';

export default function AdminProgramEditScreen() {
  const raw = useLocalSearchParams<{ id: string | string[] }>();
  const id = normalizeRouteParam(raw.id);
  const queryClient = useQueryClient();
  const [error, setError] = useState('');

  const programQuery = useQuery({
    queryKey: ['program', id],
    queryFn: () => getProgram(id!),
    enabled: !!id,
  });
  const categoriesQuery = useQuery({ queryKey: ['categories'], queryFn: getCategories });
  const exercisesQuery = useQuery({ queryKey: ['exercises'], queryFn: getExercises });

  const initial: ProgramFormValues | null = useMemo(() => {
    const program = programQuery.data;
    if (!program || !categoriesQuery.data || !exercisesQuery.data) return null;
    return {
      name: program.name,
      description: program.description,
      durationMinutes: String(program.durationMinutes),
      estimatedCalories: String(program.estimatedCalories),
      difficulty: program.difficulty,
      categoryId: program.categoryId || categoriesQuery.data[0]?.id || '',
      isFeatured: program.isFeatured,
      imageUrl: program.imageUrl ?? '',
      exercises:
        program.exercises.length > 0
          ? program.exercises.map((ex) => ({
              key: ex.id,
              exerciseId: ex.exerciseId,
              sets: String(ex.sets),
              reps: String(ex.reps),
              durationSeconds: ex.durationSeconds != null ? String(ex.durationSeconds) : '',
            }))
          : [emptyExerciseRow(exercisesQuery.data[0]?.id ?? '')],
    };
  }, [programQuery.data, categoriesQuery.data, exercisesQuery.data]);

  const mutation = useMutation({
    mutationFn: (payload: Parameters<typeof updateProgram>[1]) => updateProgram(id!, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['programs'] });
      await queryClient.invalidateQueries({ queryKey: ['program', id] });
      router.replace(ROUTES.adminPrograms);
    },
    onError: (err) => setError(parseApiError(err).message),
  });

  if (!id) {
    return (
      <AdminGuard>
        <QueryState message="Missing program id." onRetry={() => router.replace(ROUTES.adminPrograms)} />
      </AdminGuard>
    );
  }

  return (
    <AdminGuard>
      <View style={styles.screen}>
        <ScreenHeader title="Edit program" onBack={() => router.back()} />
        {programQuery.isError || categoriesQuery.isError || exercisesQuery.isError ? (
          <QueryState
            message={getApiErrorMessage(
              programQuery.error || categoriesQuery.error || exercisesQuery.error
            )}
            onRetry={() => {
              void programQuery.refetch();
              void categoriesQuery.refetch();
              void exercisesQuery.refetch();
            }}
          />
        ) : !initial ? (
          <View style={styles.centered}>
            <ActivityIndicator color={theme.colors.primary} size="large" />
          </View>
        ) : (
          <ProgramForm
            key={id}
            categories={categoriesQuery.data ?? []}
            exercises={exercisesQuery.data ?? []}
            initial={initial}
            submitLabel="Save changes"
            loading={mutation.isPending}
            error={error}
            onSubmit={(payload) => {
              setError('');
              mutation.mutate(payload);
            }}
            onCancel={() => router.back()}
          />
        )}
      </View>
    </AdminGuard>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  centered: { flex: 1, alignItems: 'center', justifyContent: 'center' },
});
