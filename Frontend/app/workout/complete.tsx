import { useQueryClient } from '@tanstack/react-query';
import { router } from 'expo-router';
import { BarChart3, CheckCircle2, Clock, Flame, Heart, Share2 } from 'lucide-react-native';
import { useEffect, useState } from 'react';
import { Alert, Pressable, ScrollView, Share, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { CompletedExerciseRow } from '@/components/workout/CompletedExerciseRow';
import { SummaryStat } from '@/components/workout/SummaryStat';
import { Button } from '@/components/ui/Button';
import { AuthGuard } from '@/components/auth/AuthGuard';
import { theme } from '@/constants/theme';
import { formatHms } from '@/lib/workoutTimer';
import { firstName } from '@/lib/format';
import { useAuthStore } from '@/stores/authStore';
import { useWorkoutCompleteStore } from '@/stores/workoutCompleteStore';

export default function WorkoutCompleteScreen() {
  return (
    <AuthGuard>
      <WorkoutCompleteContent />
    </AuthGuard>
  );
}

function WorkoutCompleteContent() {
  const insets = useSafeAreaInsets();
  const user = useAuthStore((s) => s.user);
  const summary = useWorkoutCompleteStore((s) => s.summary);
  const hydrateSummary = useWorkoutCompleteStore((s) => s.hydrateSummary);
  const clearSummary = useWorkoutCompleteStore((s) => s.clearSummary);
  const queryClient = useQueryClient();
  const [hydrated, setHydrated] = useState(!!summary);

  useEffect(() => {
    if (summary) {
      setHydrated(true);
      return;
    }
    void hydrateSummary().finally(() => setHydrated(true));
  }, [summary, hydrateSummary]);

  if (!hydrated) {
    return (
      <View style={[styles.centered, { paddingTop: insets.top }]}>
        <Text style={styles.empty}>Loading summary…</Text>
      </View>
    );
  }

  if (!summary) {
    return (
      <View style={[styles.centered, { paddingTop: insets.top }]}>
        <Text style={styles.empty}>No workout summary available.</Text>
        <Button title="Go Home" onPress={() => router.replace('/(tabs)')} />
      </View>
    );
  }

  const handleDone = () => {
    void clearSummary();
    router.replace('/(tabs)');
  };

  const handleHistory = () => {
    void clearSummary();
    queryClient.invalidateQueries({ queryKey: ['progress'] });
    router.replace('/(tabs)/progress');
  };

  const handleShare = async () => {
    try {
      await Share.share({
        message: `I just completed ${summary.programName} on ATHLO! ${formatHms(summary.durationSeconds)} · ${summary.caloriesBurned} kcal burned 💪`,
      });
    } catch {
      Alert.alert('Share', 'Unable to share right now.');
    }
  };

  return (
    <View style={[styles.screen, { paddingTop: insets.top }]}>
      <View style={styles.topBar}>
        <View style={{ width: 48 }} />
        <Text style={styles.topTitle}>Summary</Text>
        <Pressable onPress={handleDone} hitSlop={8}>
          <Text style={styles.done}>Done</Text>
        </Pressable>
      </View>

      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.hero}>
          <CheckCircle2 color={theme.colors.primary} size={56} />
          <Text style={styles.title}>Workout Complete!</Text>
          <Text style={styles.subtitle}>Great job, {firstName(user?.fullName ?? 'Athlete')}! 💪</Text>
        </View>

        <View style={styles.statsGrid}>
          <SummaryStat
            icon={Clock}
            iconColor={theme.colors.primary}
            value={formatHms(summary.durationSeconds)}
            label="Total Time"
            sublabel="hh:mm:ss"
          />
          <SummaryStat
            icon={Flame}
            iconColor={theme.colors.orange}
            value={`${summary.caloriesBurned} kcal`}
            label="Total Calories"
          />
          <SummaryStat
            icon={Heart}
            iconColor={theme.colors.green}
            value={`${summary.avgHeartRate} BPM`}
            label="Avg. Heart Rate"
            sublabel="Estimate"
          />
          <SummaryStat
            icon={BarChart3}
            iconColor={theme.colors.purple}
            value={`${summary.intensityPercent}%`}
            label="Workout Intensity"
            sublabel="Estimate"
          />
        </View>

        <View style={styles.sectionHeader}>
          <Text style={styles.sectionTitle}>Exercises Completed</Text>
          <Text style={styles.sectionMeta}>
            {summary.exercises.length} of {summary.exercises.length}
          </Text>
        </View>

        {summary.exercises.length > 0 ? (
          summary.exercises.map((ex) => <CompletedExerciseRow key={ex.id} exercise={ex} />)
        ) : (
          <Text style={styles.emptyList}>Exercises recorded for this session.</Text>
        )}

        <View style={styles.performance}>
          <Text style={styles.sectionTitle}>Workout Performance</Text>
          <Text style={styles.performanceNote}>Illustrative chart — not recorded from a device.</Text>
          <View style={styles.performanceRow}>
            <Text style={styles.performanceLabel}>Performance Over Time</Text>
            <Text style={styles.performanceGood}>Great progress!</Text>
          </View>
          <View style={styles.chartPlaceholder}>
            <View style={styles.chartLine} />
          </View>
        </View>

        <Button title="View Workout History" onPress={handleHistory} />
        <Pressable style={styles.shareBtn} onPress={handleShare}>
          <Share2 color={theme.colors.primary} size={18} />
          <Text style={styles.shareText}>Share Your Workout</Text>
        </Pressable>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  topBar: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
  },
  topTitle: { color: theme.colors.text, fontWeight: '600', fontSize: 16 },
  done: { color: theme.colors.primary, fontWeight: '600', fontSize: 16 },
  content: { padding: theme.spacing.md, paddingBottom: 40 },
  hero: { alignItems: 'center', marginBottom: theme.spacing.lg },
  title: { color: theme.colors.text, fontSize: 26, fontWeight: '800', marginTop: theme.spacing.md },
  subtitle: { color: theme.colors.textMuted, marginTop: 6, fontSize: 16 },
  statsGrid: { flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between', marginBottom: theme.spacing.lg },
  sectionHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: theme.spacing.sm },
  sectionTitle: { color: theme.colors.text, fontSize: 18, fontWeight: '700' },
  sectionMeta: { color: theme.colors.textMuted, fontSize: 13 },
  emptyList: { color: theme.colors.textMuted, marginBottom: theme.spacing.md },
  performance: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginVertical: theme.spacing.lg,
  },
  performanceNote: { color: theme.colors.textMuted, fontSize: 12, marginTop: 4 },
  performanceRow: { flexDirection: 'row', justifyContent: 'space-between', marginTop: 8, marginBottom: 12 },
  performanceLabel: { color: theme.colors.textMuted, fontSize: 13 },
  performanceGood: { color: theme.colors.green, fontSize: 13, fontWeight: '600' },
  chartPlaceholder: {
    height: 100,
    backgroundColor: theme.colors.surfaceLight,
    borderRadius: theme.radius.md,
    overflow: 'hidden',
    justifyContent: 'flex-end',
  },
  chartLine: {
    height: '65%',
    backgroundColor: theme.colors.green,
    opacity: 0.35,
    borderTopLeftRadius: theme.radius.md,
    borderTopRightRadius: theme.radius.md,
  },
  shareBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    marginTop: theme.spacing.md,
    paddingVertical: 14,
  },
  shareText: { color: theme.colors.text, fontWeight: '600', fontSize: 15 },
  centered: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: theme.colors.background,
    padding: theme.spacing.lg,
    gap: theme.spacing.md,
  },
  empty: { color: theme.colors.textMuted },
});
