import { router } from 'expo-router';
import { Calendar, Dumbbell, Signal, Star } from 'lucide-react-native';
import { Image, StyleSheet, Text, View } from 'react-native';
import { Button } from '@/components/ui/Button';
import { theme } from '@/constants/theme';
import type { ProgramListItem } from '@/lib/types';

interface FeaturedProgramCardProps {
  program: ProgramListItem;
}

export function FeaturedProgramCard({ program }: FeaturedProgramCardProps) {
  return (
    <View style={styles.card}>
      <View style={styles.row}>
        <View style={styles.imageWrap}>
          {program.imageUrl ? (
            <Image source={{ uri: program.imageUrl }} style={styles.image} />
          ) : (
            <View style={styles.imagePlaceholder}>
              <Dumbbell color={theme.colors.primary} size={40} />
            </View>
          )}
          <View style={styles.rating}>
            <Star color={theme.colors.yellow} size={12} fill={theme.colors.yellow} />
            <Text style={styles.ratingText}>4.8</Text>
          </View>
        </View>
        <View style={styles.content}>
          <Text style={styles.eyebrow}>FEATURED PROGRAM</Text>
          <Text style={styles.title} numberOfLines={2}>{program.name}</Text>
          <View style={styles.stat}>
            <Calendar color={theme.colors.textMuted} size={13} />
            <Text style={styles.statText}>{program.durationMinutes} min program</Text>
          </View>
          <View style={styles.stat}>
            <Signal color={theme.colors.textMuted} size={13} />
            <Text style={styles.statText}>{program.difficulty}</Text>
          </View>
          <View style={styles.stat}>
            <Dumbbell color={theme.colors.textMuted} size={13} />
            <Text style={styles.statText}>{program.exerciseCount} exercises</Text>
          </View>
        </View>
      </View>
      <Button title="View Program" onPress={() => router.push(`/program/${program.id}`)} />
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.xl,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginBottom: theme.spacing.lg,
    gap: theme.spacing.md,
  },
  row: { flexDirection: 'row', gap: theme.spacing.md },
  imageWrap: { width: 120, height: 140, borderRadius: theme.radius.lg, overflow: 'hidden' },
  image: { width: '100%', height: '100%' },
  imagePlaceholder: {
    flex: 1,
    backgroundColor: theme.colors.surfaceLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  rating: {
    position: 'absolute',
    bottom: 8,
    left: 8,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: 'rgba(0,0,0,0.6)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: theme.radius.full,
  },
  ratingText: { color: theme.colors.text, fontSize: 11, fontWeight: '700' },
  content: { flex: 1, gap: 4 },
  eyebrow: {
    color: theme.colors.primary,
    fontSize: 10,
    fontWeight: '700',
    letterSpacing: 0.8,
  },
  title: { color: theme.colors.text, fontSize: 18, fontWeight: '700', marginBottom: 4 },
  stat: { flexDirection: 'row', alignItems: 'center', gap: 6 },
  statText: { color: theme.colors.textMuted, fontSize: 12 },
});
