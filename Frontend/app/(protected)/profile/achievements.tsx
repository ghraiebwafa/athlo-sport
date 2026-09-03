import { useQuery } from '@tanstack/react-query';
import { router } from 'expo-router';
import { Dumbbell, Flame, LucideIcon, Trophy } from 'lucide-react-native';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';
import { QueryState } from '@/components/ui/QueryState';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { getAchievements } from '@/lib/api/achievements';
import { getApiErrorMessage } from '@/lib/api/client';
import { ROUTES } from '@/lib/routes';

const icons: Record<string, LucideIcon> = {
  first: Dumbbell,
  streak7: Flame,
  workouts25: Dumbbell,
  calories10k: Trophy,
};

export default function AchievementsScreen() {
  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['achievements'],
    queryFn: getAchievements,
  });

  if (isLoading) {
    return (
      <View style={styles.screen}>
        <ScreenHeader title="Achievements" onBack={() => router.back()} />
        <View style={styles.centered}>
          <ActivityIndicator color={theme.colors.primary} size="large" />
        </View>
      </View>
    );
  }

  if (isError || !data) {
    return (
      <View style={styles.screen}>
        <ScreenHeader title="Achievements" onBack={() => router.back()} />
        <QueryState
          message={getApiErrorMessage(error) || 'Unable to load achievements.'}
          onRetry={() => void refetch()}
        />
      </View>
    );
  }

  const unlocked = data.filter((a) => a.unlocked).length;

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Achievements" onBack={() => router.back()} />
      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.summary}>
          {unlocked} of {data.length} unlocked
        </Text>
        {data.map((item) => {
          const Icon = icons[item.key] ?? Trophy;
          return (
            <View key={item.key} style={[styles.card, !item.unlocked && styles.cardLocked]}>
              <View style={[styles.iconWrap, { borderColor: item.color }]}>
                <Icon color={item.unlocked ? item.color : theme.colors.textMuted} size={22} />
              </View>
              <View style={styles.body}>
                <Text style={styles.title}>{item.title}</Text>
                <Text style={styles.subtitle}>
                  {item.unlocked ? item.subtitle : 'Keep training to unlock'}
                </Text>
              </View>
              <Text style={[styles.status, item.unlocked && styles.statusOn]}>
                {item.unlocked ? 'Unlocked' : 'Locked'}
              </Text>
            </View>
          );
        })}
        <Text style={styles.hint} onPress={() => router.push(ROUTES.progress)}>
          See progress details →
        </Text>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  centered: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  content: { padding: theme.spacing.md, paddingBottom: 40, gap: theme.spacing.sm },
  summary: { color: theme.colors.textMuted, marginBottom: theme.spacing.sm },
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
  },
  cardLocked: { opacity: 0.55 },
  iconWrap: {
    width: 48,
    height: 48,
    borderRadius: 14,
    borderWidth: 2,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: theme.colors.surfaceLight,
  },
  body: { flex: 1 },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  subtitle: { color: theme.colors.textMuted, fontSize: 13, marginTop: 2 },
  status: { color: theme.colors.textMuted, fontSize: 12, fontWeight: '600' },
  statusOn: { color: theme.colors.green },
  hint: { color: theme.colors.primary, fontWeight: '600', marginTop: theme.spacing.md, textAlign: 'center' },
});
