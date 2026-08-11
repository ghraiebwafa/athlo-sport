import { router } from 'expo-router';
import { Clock, Dumbbell } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { Button } from '@/components/ui/Button';
import { theme } from '@/constants/theme';
import { programDetail, ROUTES } from '@/lib/routes';
import type { ProgramListItem, WorkoutSession } from '@/lib/types';

interface TodayWorkoutCardProps {
  program: ProgramListItem;
  activeWorkout?: WorkoutSession | null;
}

export function TodayWorkoutCard({ program, activeWorkout }: TodayWorkoutCardProps) {
  const handleStart = () => {
    if (activeWorkout) {
      router.push(ROUTES.activeWorkout);
      return;
    }
    router.push(programDetail(program.id));
  };

  return (
    <View style={styles.card}>
      <View style={styles.imagePlaceholder}>
        <Dumbbell color={theme.colors.primary} size={48} />
      </View>
      <View style={styles.body}>
        <Text style={styles.eyebrow}>{activeWorkout ? 'IN PROGRESS' : "TODAY'S WORKOUT"}</Text>
        <Text style={styles.title}>{activeWorkout?.programName ?? program.name}</Text>
        <View style={styles.meta}>
          <View style={styles.metaItem}>
            <Clock color={theme.colors.textMuted} size={14} />
            <Text style={styles.metaText}>{program.durationMinutes} min</Text>
          </View>
          <View style={styles.metaItem}>
            <Dumbbell color={theme.colors.primary} size={14} />
            <Text style={[styles.metaText, styles.metaAccent]}>{program.difficulty}</Text>
          </View>
        </View>
        <Button
          title={activeWorkout ? 'Resume Workout →' : 'Start Workout →'}
          onPress={handleStart}
        />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.xl,
    overflow: 'hidden',
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginBottom: theme.spacing.md,
  },
  imagePlaceholder: {
    height: 120,
    backgroundColor: theme.colors.surfaceLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  body: { padding: theme.spacing.md },
  eyebrow: {
    color: theme.colors.primary,
    fontSize: 11,
    fontWeight: '700',
    letterSpacing: 1,
    marginBottom: 6,
  },
  title: { color: theme.colors.text, fontSize: 22, fontWeight: '700', marginBottom: 12 },
  meta: { flexDirection: 'row', gap: theme.spacing.lg, marginBottom: theme.spacing.md },
  metaItem: { flexDirection: 'row', alignItems: 'center', gap: 6 },
  metaText: { color: theme.colors.textMuted, fontSize: 13 },
  metaAccent: { color: theme.colors.primary, fontWeight: '600' },
});
