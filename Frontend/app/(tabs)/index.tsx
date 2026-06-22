import { useQuery } from '@tanstack/react-query';
import { router } from 'expo-router';
import { Flame, Footprints, Trophy } from 'lucide-react-native';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { TodayWorkoutCard } from '@/components/home/TodayWorkoutCard';
import { StatTile } from '@/components/home/StatTile';
import { UpcomingWorkoutRow } from '@/components/home/UpcomingWorkoutRow';
import { WeeklyGoalCard } from '@/components/home/WeeklyGoalCard';
import { theme } from '@/constants/theme';
import { getApiErrorMessage } from '@/lib/api/client';
import {
  caloriesToday,
  firstName,
  formatLongDate,
  getGreeting,
  workoutsThisWeek,
} from '@/lib/format';
import { getPrograms } from '@/lib/api/programs';
import { getProgress } from '@/lib/api/progress';
import { getActiveWorkout } from '@/lib/api/workouts';
import { useAuthStore } from '@/stores/authStore';

export default function HomeScreen() {
  const user = useAuthStore((s) => s.user);

  const progressQuery = useQuery({ queryKey: ['progress'], queryFn: getProgress });
  const programsQuery = useQuery({ queryKey: ['programs'], queryFn: getPrograms });
  const activeQuery = useQuery({ queryKey: ['activeWorkout'], queryFn: getActiveWorkout });

  const isLoading = progressQuery.isLoading || programsQuery.isLoading;

  if (isLoading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator color={theme.colors.primary} size="large" />
      </View>
    );
  }

  if (progressQuery.isError || programsQuery.isError) {
    return (
      <View style={styles.centered}>
        <Text style={styles.error}>
          {getApiErrorMessage(progressQuery.error ?? programsQuery.error)}
        </Text>
      </View>
    );
  }

  const progress = progressQuery.data!;
  const programs = programsQuery.data ?? [];
  const featured = programs.find((p) => p.isFeatured) ?? programs[0];
  const upcoming = programs.find((p) => p.id !== featured?.id) ?? programs[1];
  const todayCalories = caloriesToday(progress.recentWorkouts);
  const weekCount = workoutsThisWeek(progress.weeklyFrequency);

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <View style={styles.header}>
        <View>
          <Text style={styles.greeting}>{getGreeting()},</Text>
          <Text style={styles.name}>
            {firstName(user?.fullName ?? 'Athlete')} 👋
          </Text>
          <Text style={styles.date}>{formatLongDate()}</Text>
        </View>
        <Pressable style={styles.avatar} onPress={() => router.push('/(tabs)/profile')}>
          <Text style={styles.avatarText}>
            {(user?.fullName ?? 'A').charAt(0).toUpperCase()}
          </Text>
        </Pressable>
      </View>

      <View style={styles.statsRow}>
        <StatTile
          icon={Flame}
          iconColor={theme.colors.orange}
          label="Calories Burned"
          value={String(todayCalories || progress.totalCaloriesBurned)}
          unit="kcal"
        />
        <StatTile
          icon={Footprints}
          iconColor={theme.colors.primary}
          label="Steps Today"
          value="—"
        />
        <StatTile
          icon={Trophy}
          iconColor={theme.colors.yellow}
          label="Current Streak"
          value={String(progress.currentStreak)}
          unit="Days"
        />
      </View>

      {featured ? (
        <TodayWorkoutCard program={featured} activeWorkout={activeQuery.data} />
      ) : null}

      <WeeklyGoalCard completed={weekCount} />

      {upcoming ? (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Upcoming</Text>
          <UpcomingWorkoutRow program={upcoming} />
        </View>
      ) : null}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: theme.colors.background },
  content: { padding: theme.spacing.md, paddingBottom: theme.spacing.xl },
  centered: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: theme.colors.background },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: theme.spacing.lg,
  },
  greeting: { color: theme.colors.textMuted, fontSize: 16 },
  name: { color: theme.colors.text, fontSize: 28, fontWeight: '700', marginTop: 2 },
  date: { color: theme.colors.textMuted, fontSize: 14, marginTop: 4 },
  avatar: {
    width: 48,
    height: 48,
    borderRadius: 24,
    backgroundColor: theme.colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarText: { color: '#fff', fontSize: 20, fontWeight: '700' },
  statsRow: { flexDirection: 'row', gap: theme.spacing.sm, marginBottom: theme.spacing.lg },
  section: { marginTop: theme.spacing.sm },
  sectionTitle: { color: theme.colors.text, fontSize: 18, fontWeight: '700', marginBottom: theme.spacing.sm },
  error: { color: theme.colors.error, padding: theme.spacing.md },
});
