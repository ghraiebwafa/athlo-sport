import { Dumbbell } from 'lucide-react-native';
import { Image, StyleSheet, Text, View } from 'react-native';
import Svg, { Circle } from 'react-native-svg';
import { theme } from '@/constants/theme';
import type { ProgramExercise } from '@/lib/types';

interface CurrentExerciseCardProps {
  exercise: ProgramExercise;
  setLabel: string;
  repProgress: number;
}

export function CurrentExerciseCard({ exercise, setLabel, repProgress }: CurrentExerciseCardProps) {
  const radius = 28;
  const stroke = 4;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (repProgress / 100) * circumference;

  return (
    <View style={styles.card}>
      {exercise.imageUrl ? (
        <Image source={{ uri: exercise.imageUrl }} style={styles.thumb} />
      ) : (
        <View style={styles.thumbPlaceholder}>
          <Dumbbell color={theme.colors.primary} size={28} />
        </View>
      )}
      <View style={styles.body}>
        <Text style={styles.name}>{exercise.name}</Text>
        <Text style={styles.set}>{setLabel}</Text>
        <Text style={styles.reps}>
          {exercise.durationSeconds
            ? `${exercise.durationSeconds} sec hold`
            : `${Math.floor((repProgress / 100) * exercise.reps)} / ${exercise.reps} reps`}
        </Text>
      </View>
      <View style={styles.ringWrap}>
        <Svg width={64} height={64}>
          <Circle cx={32} cy={32} r={radius} stroke={theme.colors.surfaceLight} strokeWidth={stroke} fill="none" />
          <Circle
            cx={32}
            cy={32}
            r={radius}
            stroke={theme.colors.primary}
            strokeWidth={stroke}
            fill="none"
            strokeDasharray={`${circumference} ${circumference}`}
            strokeDashoffset={offset}
            strokeLinecap="round"
            rotation="-90"
            origin="32, 32"
          />
        </Svg>
        <Text style={styles.ringText}>{repProgress}%</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.xl,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    gap: theme.spacing.md,
    marginBottom: theme.spacing.md,
  },
  thumb: { width: 72, height: 72, borderRadius: theme.radius.md },
  thumbPlaceholder: {
    width: 72,
    height: 72,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.surfaceLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  body: { flex: 1 },
  name: { color: theme.colors.text, fontWeight: '700', fontSize: 17 },
  set: { color: theme.colors.primary, fontSize: 13, fontWeight: '600', marginTop: 4 },
  reps: { color: theme.colors.textMuted, fontSize: 13, marginTop: 2 },
  ringWrap: { width: 64, height: 64, alignItems: 'center', justifyContent: 'center' },
  ringText: {
    position: 'absolute',
    color: theme.colors.text,
    fontSize: 11,
    fontWeight: '700',
  },
});
