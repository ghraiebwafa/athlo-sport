import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { router, useLocalSearchParams } from 'expo-router';
import { useEffect, useState } from 'react';
import { ActivityIndicator, Alert, ScrollView, StyleSheet, Text, View } from 'react-native';
import { AuthGuard } from '@/components/auth/AuthGuard';
import { ExerciseListItem } from '@/components/program/ExerciseListItem';
import { ProgramActions } from '@/components/program/ProgramActions';
import { ProgramHero } from '@/components/program/ProgramHero';
import { ProgramStatsRow } from '@/components/program/ProgramStatsRow';
import { theme } from '@/constants/theme';
import { getApiErrorMessage } from '@/lib/api/client';
import { getProgram } from '@/lib/api/programs';
import { isProgramSaved, toggleSavedProgram } from '@/lib/savedPrograms';
import { getActiveWorkout, startWorkout } from '@/lib/api/workouts';

export default function ProgramDetailScreen() {
  return (
    <AuthGuard>
      <ProgramDetailContent />
    </AuthGuard>
  );
}

function ProgramDetailContent() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const queryClient = useQueryClient();
  const [saved, setSaved] = useState(false);

  const programQuery = useQuery({
    queryKey: ['program', id],
    queryFn: () => getProgram(id!),
    enabled: !!id,
  });

  useEffect(() => {
    if (id) isProgramSaved(id).then(setSaved);
  }, [id]);

  const startMutation = useMutation({
    mutationFn: () => startWorkout(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activeWorkout'] });
      router.push('/workout/active');
    },
    onError: (err) => Alert.alert('Could not start workout', getApiErrorMessage(err)),
  });

  const handleStart = async () => {
    try {
      const active = await getActiveWorkout();
      if (active) {
        Alert.alert('Active workout', 'You already have a workout in progress.', [
          { text: 'Cancel', style: 'cancel' },
          { text: 'Resume', onPress: () => router.push('/workout/active') },
        ]);
        return;
      }
      startMutation.mutate();
    } catch (err) {
      Alert.alert('Error', getApiErrorMessage(err));
    }
  };

  const handleSave = async () => {
    if (!id) return;
    const next = await toggleSavedProgram(id);
    setSaved(next);
    Alert.alert(next ? 'Saved' : 'Removed', next ? 'Program saved for later.' : 'Program removed from saved.');
  };

  if (programQuery.isLoading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator color={theme.colors.primary} size="large" />
      </View>
    );
  }

  if (programQuery.isError || !programQuery.data) {
    return (
      <View style={styles.centered}>
        <Text style={styles.error}>{getApiErrorMessage(programQuery.error)}</Text>
      </View>
    );
  }

  const program = programQuery.data;
  const exerciseCount = program.exercises.length;

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <ProgramHero
        title={program.name}
        imageUrl={program.imageUrl}
        saved={saved}
        onToggleSave={handleSave}
      />

      <View style={styles.body}>
        <ProgramStatsRow
          durationMinutes={program.durationMinutes}
          difficulty={program.difficulty}
          calories={program.estimatedCalories}
          exerciseCount={exerciseCount}
        />

        <Text style={styles.sectionTitle}>About This Workout</Text>
        <Text style={styles.description}>{program.description}</Text>

        <View style={styles.exerciseHeader}>
          <Text style={styles.sectionTitle}>Exercises</Text>
          <Text style={styles.exerciseCount}>0 of {exerciseCount} completed</Text>
        </View>

        {program.exercises.map((ex) => (
          <ExerciseListItem key={ex.id} exercise={ex} />
        ))}

        <ProgramActions
          onStart={handleStart}
          onSave={handleSave}
          starting={startMutation.isPending}
          saved={saved}
        />
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: theme.colors.background },
  content: { paddingBottom: 40 },
  body: { padding: theme.spacing.md },
  centered: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: theme.colors.background },
  sectionTitle: { fontSize: 18, fontWeight: '700', color: theme.colors.text },
  description: { color: theme.colors.textMuted, lineHeight: 22, marginTop: 8, marginBottom: theme.spacing.lg },
  exerciseHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: theme.spacing.sm,
  },
  exerciseCount: { color: theme.colors.textMuted, fontSize: 13 },
  error: { color: theme.colors.error },
});
