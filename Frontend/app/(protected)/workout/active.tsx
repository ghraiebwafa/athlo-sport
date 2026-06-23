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
  View,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { CurrentExerciseCard } from '@/components/workout/CurrentExerciseCard';
import { MusicBar } from '@/components/workout/MusicBar';
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
  exerciseProgress,
  formatHms,
  formatMmSs,
} from '@/lib/workoutTimer';
import { cancelWorkout, completeWorkout, getActiveWorkout } from '@/lib/api/workouts';
import { ROUTES } from '@/lib/routes';
import { useWorkoutCompleteStore } from '@/stores/workoutCompleteStore';

export default function ActiveWorkoutScreen() {
  const insets = useSafeAreaInsets();
  const queryClient = useQueryClient();
  const setSummary = useWorkoutCompleteStore((s) => s.setSummary);
  const [now, setNow] = useState(Date.now());
  const [paused, setPaused] = useState(false);
  const pausedAtRef = useRef<number | null>(null);
  const pausedTotalRef = useRef(0);

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

  const completeMutation = useMutation({
    mutationFn: (calories: number) => completeWorkout(session!.id, calories),
    onSuccess: (data, calories) => {
      queryClient.invalidateQueries({ queryKey: ['activeWorkout'] });
      queryClient.invalidateQueries({ queryKey: ['progress'] });
      const exercises = programQuery.data?.exercises ?? [];
      const elapsed = elapsedSeconds(session!.startedAt, now) - pausedTotalRef.current;
      void setSummary({
        sessionId: data.id,
        programName: data.programName,
        durationSeconds: data.durationMinutes ? data.durationMinutes * 60 : elapsed,
        caloriesBurned: data.caloriesBurned ?? calories,
        avgHeartRate: estimateHeartRate(elapsed),
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

  const metrics = useMemo(() => {
    if (!session) return null;
    const elapsed = elapsedSeconds(session.startedAt, now) - pausedTotalRef.current;
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
  }, [session, now, programQuery.data]);

  const exerciseState = useMemo(() => {
    const exercises = programQuery.data?.exercises ?? [];
    if (!metrics || exercises.length === 0) {
      return { current: exercises[0], next: exercises[1], setLabel: 'Set 1 of 1', repProgress: 0 };
    }
    const { index, progressPercent } = exerciseProgress(
      metrics.elapsed,
      programQuery.data?.durationMinutes ?? 30,
      exercises.length
    );
    const current = exercises[index];
    const setNum = Math.min(current.sets, Math.floor(progressPercent / (100 / current.sets)) + 1);
    return {
      current,
      next: exercises[index + 1],
      setLabel: `Set ${setNum} of ${current.sets}`,
      repProgress: progressPercent,
    };
  }, [metrics, programQuery.data]);

  const togglePause = () => {
    if (paused) {
      if (pausedAtRef.current) {
        pausedTotalRef.current += Math.floor((Date.now() - pausedAtRef.current) / 1000);
        pausedAtRef.current = null;
      }
      setPaused(false);
    } else {
      pausedAtRef.current = Date.now();
      setPaused(true);
    }
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

  return (
    <View style={[styles.screen, { paddingTop: insets.top }]}>
      <View style={styles.header}>
        <Pressable onPress={() => router.replace(ROUTES.home)} hitSlop={8}>
          <ChevronLeft color={theme.colors.text} size={26} />
        </Pressable>
        <Text style={styles.headerTitle}>Active Workout</Text>
        <Pressable onPress={handleEnd} hitSlop={8}>
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
            value={String(metrics.heartRate)}
            unit="BPM"
            label="Heart Rate (est.)"
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

        {exerciseState.current ? (
          <CurrentExerciseCard
            exercise={exerciseState.current}
            setLabel={exerciseState.setLabel}
            repProgress={exerciseState.repProgress}
          />
        ) : null}

        {exerciseState.next ? <UpNextCard exercise={exerciseState.next} /> : null}

        <WorkoutProgressBar percent={metrics.progress} />

        <WorkoutControls paused={paused} onTogglePause={togglePause} />

        <MusicBar />
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
