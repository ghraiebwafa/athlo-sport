import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { router } from 'expo-router';
import { Activity, ChevronLeft, Flame, Heart, Timer } from 'lucide-react-native';
import { useEffect, useMemo, useRef, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { CurrentExerciseCard } from '@/components/workout/CurrentExerciseCard';
import { RestTimer, DEFAULT_REST_SECONDS } from '@/components/workout/RestTimer';
import { SetLogger } from '@/components/workout/SetLogger';
import { UpNextCard } from '@/components/workout/UpNextCard';
import { WorkoutControls } from '@/components/workout/WorkoutControls';
import { WorkoutProgressBar } from '@/components/workout/WorkoutProgressBar';
import { WorkoutStatChip } from '@/components/workout/WorkoutStatChip';
import { Button } from '@/components/ui/Button';
import { theme } from '@/constants/theme';
import { getApiErrorMessage } from '@/lib/api/client';
import { getProgram } from '@/lib/api/programs';
import {
  elapsedSeconds,
  estimateCaloriesBurned,
  estimateHeartRate,
  estimateIntensity,
  formatHms,
  formatMmSs,
} from '@/lib/workoutTimer';
import { cancelWorkout, completeWorkout, getActiveWorkout, logWorkoutSet } from '@/lib/api/workouts';
import { ROUTES } from '@/lib/routes';
import { usePreferencesStore } from '@/stores/preferencesStore';
import { useWorkoutCompleteStore } from '@/stores/workoutCompleteStore';

export default function ActiveWorkoutScreen() {
  const insets = useSafeAreaInsets();
  const queryClient = useQueryClient();
  const setSummary = useWorkoutCompleteStore((s) => s.setSummary);
  const [now, setNow] = useState(() => Date.now());
  const [paused, setPaused] = useState(false);
  const pausedAtRef = useRef<number | null>(null);
  const [pausedTotal, setPausedTotal] = useState(0);
  const [exerciseIndex, setExerciseIndex] = useState(0);
  const [restRemaining, setRestRemaining] = useState<number | null>(null);
  const [lastRestDuration, setLastRestDuration] = useState(DEFAULT_REST_SECONDS);
  const [pendingAdvance, setPendingAdvance] = useState(false);
  const [manualHr, setManualHr] = useState('');
  const heartRateSource = usePreferencesStore((s) => s.heartRateSource);

  const { data: session, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['activeWorkout'],
    queryFn: getActiveWorkout,
  });

  const programQuery = useQuery({
    queryKey: ['program', session?.programId],
    queryFn: () => getProgram(session!.programId),
    enabled: !!session?.programId,
  });

  useEffect(() => {
    if (!session || paused) return;
    const timer = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(timer);
  }, [session, paused]);

  const exercises = programQuery.data?.exercises ?? [];
  const loggedSets = session?.sets ?? [];

  const isResting = restRemaining != null;
  useEffect(() => {
    if (!isResting || paused) return;
    const timer = setInterval(() => {
      setRestRemaining((prev) => {
        if (prev == null || prev <= 1) return null;
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(timer);
  }, [isResting, paused]);

  useEffect(() => {
    if (isResting || !pendingAdvance) return;
    setPendingAdvance(false);
    setExerciseIndex((i) => Math.min(exercises.length - 1, i + 1));
  }, [isResting, pendingAdvance, exercises.length]);

  const startRest = (seconds = lastRestDuration) => {
    const next = Math.max(1, seconds);
    setLastRestDuration(next);
    setRestRemaining(next);
  };

  const endRest = () => setRestRemaining(null);

  const completeMutation = useMutation({
    mutationFn: (calories: number) => completeWorkout(session!.id, calories),
    onSuccess: (data, calories) => {
      queryClient.invalidateQueries({ queryKey: ['activeWorkout'] });
      queryClient.invalidateQueries({ queryKey: ['progress'] });
      const elapsed = elapsedSeconds(session!.startedAt, now) - pausedTotal;
      void setSummary({
        sessionId: data.id,
        programName: data.programName,
        durationSeconds: data.durationMinutes ? data.durationMinutes * 60 : elapsed,
        caloriesBurned: data.caloriesBurned ?? calories,
        avgHeartRate:
          heartRateSource === 'manual' && Number(manualHr) > 0
            ? Number(manualHr)
            : estimateHeartRate(elapsed),
        intensityPercent: estimateIntensity(
          Math.min(100, (elapsed / Math.max(1, (programQuery.data?.durationMinutes ?? 30) * 60)) * 100)
        ),
        exercises,
        completedAt: data.completedAt ?? new Date().toISOString(),
      });
      router.replace(ROUTES.completeWorkout);
    },
    onError: (err) => Alert.alert('Error', getApiErrorMessage(err)),
  });

  const cancelMutation = useMutation({
    mutationFn: () => cancelWorkout(session!.id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activeWorkout'] });
      router.replace(ROUTES.home);
    },
    onError: (err) => Alert.alert('Error', getApiErrorMessage(err)),
  });

  const logSetMutation = useMutation({
    mutationFn: (input: {
      programExerciseId: string;
      setNumber: number;
      repsCompleted: number;
      weightKg?: number;
    }) => logWorkoutSet(session!.id, input),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['activeWorkout'] });
      queryClient.invalidateQueries({ queryKey: ['progress'] });

      const exerciseIdx = exercises.findIndex((e) => e.id === variables.programExerciseId);
      const exercise = exerciseIdx >= 0 ? exercises[exerciseIdx] : undefined;
      const completedForExercise =
        loggedSets.filter((s) => s.programExerciseId === variables.programExerciseId && s.completed)
          .length + 1;
      const exerciseFinished = exercise ? completedForExercise >= exercise.sets : false;

      if (!exerciseFinished) {
        startRest();
        return;
      }

      const hasNext = exerciseIdx >= 0 && exerciseIdx < exercises.length - 1;
      if (hasNext) {
        setPendingAdvance(true);
        startRest();
      }
    },
    onError: (err) => Alert.alert('Error', getApiErrorMessage(err)),
  });

  const metrics = useMemo(() => {
    if (!session) return null;
    const elapsed = elapsedSeconds(session.startedAt, now) - pausedTotal;
    const durationMin = programQuery.data?.durationMinutes ?? 30;
    const targetCal = programQuery.data?.estimatedCalories ?? 300;
    const totalSec = durationMin * 60;
    const remaining = Math.max(0, totalSec - elapsed);
    const progress = Math.min(100, Math.floor((elapsed / Math.max(1, totalSec)) * 100));
    return {
      elapsed,
      remaining,
      progress,
      calories: estimateCaloriesBurned(elapsed, targetCal, durationMin),
      heartRate: estimateHeartRate(elapsed),
      intensity: estimateIntensity(progress),
    };
  }, [session, now, programQuery.data, pausedTotal]);

  const exerciseState = useMemo(() => {
    if (exercises.length === 0) {
      return { current: undefined, next: undefined, setLabel: 'Set 1 of 1', repProgress: 0 };
    }
    const index = Math.min(exerciseIndex, exercises.length - 1);
    const current = exercises[index];
    const completedForExercise = loggedSets.filter(
      (s) => s.programExerciseId === current.id && s.completed
    ).length;
    const setNum = Math.min(current.sets, Math.max(1, completedForExercise + 1));
    const repProgress = Math.min(100, Math.round((completedForExercise / Math.max(1, current.sets)) * 100));
    return {
      current,
      next: exercises[index + 1],
      setLabel: `Set ${setNum} of ${current.sets}`,
      repProgress,
      completedForExercise,
    };
  }, [exercises, exerciseIndex, loggedSets]);

  const togglePause = () => {
    if (paused) {
      if (pausedAtRef.current) {
        setPausedTotal((prev) => prev + Math.floor((Date.now() - pausedAtRef.current!) / 1000));
        pausedAtRef.current = null;
      }
      setPaused(false);
    } else {
      pausedAtRef.current = Date.now();
      setPaused(true);
    }
  };

  const handleBack = () => {
    Alert.alert('Leave workout?', 'Your session is still active.', [
      { text: 'Stay', style: 'cancel' },
      { text: 'Keep workout', onPress: () => router.replace(ROUTES.home) },
      {
        text: 'Discard',
        style: 'destructive',
        onPress: () => cancelMutation.mutate(),
      },
    ]);
  };

  const handleEnd = () => {
    if (!metrics || !session) return;
    Alert.alert('End workout', 'Complete this session?', [
      { text: 'Keep going', style: 'cancel' },
      {
        text: 'Complete',
        onPress: () => completeMutation.mutate(metrics.calories),
      },
      {
        text: 'Discard',
        style: 'destructive',
        onPress: () => cancelMutation.mutate(),
      },
    ]);
  };

  if (isLoading || programQuery.isLoading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator color={theme.colors.primary} size="large" />
      </View>
    );
  }

  if (isError) {
    return (
      <View style={styles.centered}>
        <Text style={styles.error}>{getApiErrorMessage(error)}</Text>
        <Button title="Retry" onPress={() => refetch()} variant="secondary" />
      </View>
    );
  }

  if (!session || !metrics) {
    return (
      <View style={styles.centered}>
        <Text style={styles.empty}>No active workout.</Text>
        <Button title="Browse Programs" onPress={() => router.replace(ROUTES.programs)} />
      </View>
    );
  }

  const currentLogs = exerciseState.current
    ? loggedSets.filter((s) => s.programExerciseId === exerciseState.current!.id)
    : [];

  return (
    <View style={[styles.screen, { paddingTop: insets.top }]}>
      <View style={styles.header}>
        <Pressable onPress={handleBack} hitSlop={8} accessibilityLabel="Leave workout">
          <ChevronLeft color={theme.colors.text} size={26} />
        </Pressable>
        <Text style={styles.headerTitle}>Active Workout</Text>
        <Pressable onPress={handleEnd} hitSlop={8} accessibilityLabel="End workout">
          <Text style={styles.end}>End</Text>
        </Pressable>
      </View>

      <ScrollView contentContainerStyle={styles.content} showsVerticalScrollIndicator={false}>
        <Text style={styles.program}>{session.programName}</Text>
        <Text style={styles.timer}>{formatHms(metrics.elapsed)}</Text>
        <Text style={styles.elapsedLabel}>Elapsed Time{paused ? ' · Paused' : ''}</Text>

        <View style={styles.statsRow}>
          <WorkoutStatChip
            icon={Heart}
            iconColor={theme.colors.red}
            value={
              heartRateSource === 'manual' && manualHr.trim()
                ? manualHr.trim()
                : String(metrics.heartRate)
            }
            unit="BPM"
            label={heartRateSource === 'manual' ? 'Heart Rate' : 'Heart Rate (est.)'}
            valueColor={theme.colors.red}
          />
          <WorkoutStatChip
            icon={Flame}
            iconColor={theme.colors.orange}
            value={String(metrics.calories)}
            unit="kcal"
            label="Calories (est.)"
            valueColor={theme.colors.orange}
          />
          <WorkoutStatChip
            icon={Activity}
            iconColor={theme.colors.green}
            value={`${metrics.intensity}%`}
            label="Intensity (est.)"
            valueColor={theme.colors.green}
          />
          <WorkoutStatChip
            icon={Timer}
            iconColor={theme.colors.primary}
            value={formatMmSs(metrics.remaining)}
            label="Time Remaining"
            valueColor={theme.colors.primary}
          />
        </View>

        {heartRateSource === 'manual' ? (
          <View style={styles.hrRow}>
            <Text style={styles.hrLabel}>Manual BPM</Text>
            <TextInput
              style={styles.hrInput}
              keyboardType="number-pad"
              value={manualHr}
              onChangeText={setManualHr}
              placeholder={String(metrics.heartRate)}
              placeholderTextColor={theme.colors.textMuted}
              accessibilityLabel="Manual heart rate"
            />
          </View>
        ) : null}

        {exerciseState.current ? (
          <>
            <CurrentExerciseCard
              exercise={exerciseState.current}
              setLabel={exerciseState.setLabel}
              repProgress={exerciseState.repProgress}
            />
            {restRemaining != null && restRemaining > 0 ? (
              <RestTimer
                remainingSeconds={restRemaining}
                paused={paused}
                title={pendingAdvance ? 'Rest · Next exercise' : 'Rest'}
                onSkip={endRest}
                onAddSeconds={(seconds) =>
                  setRestRemaining((prev) => (prev == null ? seconds : prev + seconds))
                }
                onSetDuration={(seconds) => startRest(seconds)}
              />
            ) : (
              <SetLogger
                key={exerciseState.current.id}
                exercise={exerciseState.current}
                loggedSets={currentLogs}
                busy={logSetMutation.isPending}
                onLogSet={(input) => logSetMutation.mutate(input)}
              />
            )}
            {exercises.length > 1 ? (
              <View style={styles.navRow}>
                <Button
                  title="Previous"
                  variant="secondary"
                  onPress={() => {
                    setPendingAdvance(false);
                    endRest();
                    setExerciseIndex((i) => Math.max(0, i - 1));
                  }}
                  disabled={exerciseIndex <= 0}
                />
                <Button
                  title="Next exercise"
                  variant="secondary"
                  onPress={() => {
                    setPendingAdvance(false);
                    endRest();
                    setExerciseIndex((i) => Math.min(exercises.length - 1, i + 1));
                  }}
                  disabled={exerciseIndex >= exercises.length - 1}
                />
              </View>
            ) : null}
          </>
        ) : null}

        {exerciseState.next ? <UpNextCard exercise={exerciseState.next} /> : null}

        <WorkoutProgressBar percent={metrics.progress} />

        <WorkoutControls paused={paused} onTogglePause={togglePause} />
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.md,
    paddingBottom: theme.spacing.sm,
  },
  headerTitle: { color: theme.colors.text, fontSize: 17, fontWeight: '600' },
  end: { color: theme.colors.red, fontWeight: '600', fontSize: 16 },
  content: { padding: theme.spacing.md, paddingBottom: 40 },
  program: { color: theme.colors.textMuted, textAlign: 'center', marginBottom: 4 },
  timer: {
    color: theme.colors.text,
    fontSize: 48,
    fontWeight: '800',
    textAlign: 'center',
    fontVariant: ['tabular-nums'],
  },
  elapsedLabel: { color: theme.colors.textMuted, textAlign: 'center', marginBottom: theme.spacing.lg },
  statsRow: {
    flexDirection: 'row',
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginBottom: theme.spacing.lg,
    paddingVertical: theme.spacing.sm,
  },
  navRow: { flexDirection: 'row', gap: theme.spacing.sm, marginBottom: theme.spacing.md },
  hrRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.sm,
    marginBottom: theme.spacing.md,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
  },
  hrLabel: { color: theme.colors.textMuted, fontWeight: '600', flex: 1 },
  hrInput: {
    minWidth: 80,
    color: theme.colors.text,
    fontSize: 18,
    fontWeight: '700',
    textAlign: 'right',
    paddingVertical: 6,
  },
  centered: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: theme.colors.background,
    padding: theme.spacing.lg,
    gap: theme.spacing.md,
  },
  empty: { color: theme.colors.textMuted },
  error: { color: theme.colors.error, textAlign: 'center' },
});
