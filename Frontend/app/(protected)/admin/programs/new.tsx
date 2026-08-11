import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { router } from 'expo-router';
import { ActivityIndicator, StyleSheet, View } from 'react-native';
import { AdminGuard } from '@/components/auth/AdminGuard';
import { ProgramForm, emptyExerciseRow, type ProgramFormValues } from '@/components/admin/ProgramForm';
import { QueryState } from '@/components/ui/QueryState';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { createProgram, getExercises } from '@/lib/api/admin';
import { getApiErrorMessage, parseApiError } from '@/lib/api/client';
import { getCategories } from '@/lib/api/programs';
import { ROUTES } from '@/lib/routes';
import { useMemo, useState } from 'react';

export default function AdminProgramCreateScreen() {
  const queryClient = useQueryClient();
  const [error, setError] = useState('');

  const categoriesQuery = useQuery({ queryKey: ['categories'], queryFn: getCategories });
  const exercisesQuery = useQuery({ queryKey: ['exercises'], queryFn: getExercises });

  const initial: ProgramFormValues | null = useMemo(() => {
    const categories = categoriesQuery.data;
    const exercises = exercisesQuery.data;
    if (!categories || !exercises) return null;
    return {
      name: '',
      description: '',
      durationMinutes: '45',
      estimatedCalories: '300',
      difficulty: 'Beginner',
      categoryId: categories[0]?.id ?? '',
      isFeatured: false,
      imageUrl: '',
      exercises: [emptyExerciseRow(exercises[0]?.id ?? '')],
    };
  }, [categoriesQuery.data, exercisesQuery.data]);

  const mutation = useMutation({
    mutationFn: createProgram,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['programs'] });
      router.replace(ROUTES.adminPrograms);
    },
    onError: (err) => setError(parseApiError(err).message),
  });

  return (
    <AdminGuard>
      <View style={styles.screen}>
        <ScreenHeader title="New program" onBack={() => router.back()} />
        {categoriesQuery.isError || exercisesQuery.isError ? (
          <QueryState
            message={getApiErrorMessage(categoriesQuery.error || exercisesQuery.error)}
            onRetry={() => {
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
            key={`${initial.categoryId}-${initial.exercises[0]?.exerciseId}`}
            categories={categoriesQuery.data ?? []}
            exercises={exercisesQuery.data ?? []}
            initial={initial}
            submitLabel="Create program"
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
