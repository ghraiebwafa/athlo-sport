import { useQuery } from '@tanstack/react-query';
import { router } from 'expo-router';
import {
  Bookmark,
  Calendar,
  Crown,
  Dumbbell,
  Flame,
  HelpCircle,
  KeyRound,
  Lock,
  LogOut,
  Scale,
  Settings,
  Trophy,
  UserRound,
  Zap,
} from 'lucide-react-native';
import { ActivityIndicator, Alert, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { AchievementsSection } from '@/components/profile/AchievementsSection';
import { GoalCard } from '@/components/profile/GoalCard';
import { OverviewGrid, OverviewTile } from '@/components/profile/OverviewGrid';
import { PersonalRecordsSection } from '@/components/profile/PersonalRecordsSection';
import { SettingsMenu, showComingSoon } from '@/components/profile/SettingsMenu';
import { UserProfileCard } from '@/components/profile/UserProfileCard';
import { QueryState } from '@/components/ui/QueryState';
import { theme } from '@/constants/theme';
import { ROUTES } from '@/lib/routes';
import { getProfile, logout } from '@/lib/api/auth';
import { getApiErrorMessage } from '@/lib/api/client';
import { signOutAndRedirect } from '@/lib/authSession';
import { getProgress } from '@/lib/api/progress';
import { buildAchievements, buildPersonalRecords } from '@/lib/profileHelpers';
import { getTokens, useAuthStore } from '@/stores/authStore';

const menuItems = [
  { id: 'history', label: 'Workout History', icon: Calendar },
  { id: 'saved', label: 'Saved Programs', icon: Bookmark },
  { id: 'editProfile', label: 'Edit Profile', icon: UserRound },
  { id: 'changePassword', label: 'Change Password', icon: KeyRound },
  { id: 'achievements', label: 'Achievements', icon: Trophy },
  { id: 'notifications', label: 'Notifications', icon: Settings },
  { id: 'privacy', label: 'Privacy & Security', icon: Lock },
  { id: 'subscription', label: 'Subscription', icon: Crown },
  { id: 'help', label: 'Help & Support', icon: HelpCircle },
  { id: 'logout', label: 'Log Out', icon: LogOut, destructive: true },
];

export default function ProfileScreen() {
  const user = useAuthStore((s) => s.user);

  const profileQuery = useQuery({
    queryKey: ['profile'],
    queryFn: getProfile,
    enabled: !!user,
  });

  const progressQuery = useQuery({
    queryKey: ['progress'],
    queryFn: getProgress,
    enabled: !!user,
  });

  const display = profileQuery.data ?? user;
  const progress = progressQuery.data;

  const handleLogout = async () => {
    const tokens = getTokens();
    try {
      if (tokens?.refreshToken) await logout(tokens.refreshToken);
    } catch {
      // clear local session regardless
    }
    await signOutAndRedirect();
  };

  const handleMenu = (id: string) => {
    if (id === 'logout') {
      Alert.alert('Log out', 'Are you sure you want to sign out?', [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Log Out', style: 'destructive', onPress: handleLogout },
      ]);
      return;
    }
    if (id === 'history') {
      router.push(ROUTES.workoutHistory);
      return;
    }
    if (id === 'saved') {
      router.push(ROUTES.savedPrograms);
      return;
    }
    if (id === 'editProfile') {
      router.push(ROUTES.editProfile);
      return;
    }
    if (id === 'changePassword') {
      router.push(ROUTES.changePassword);
      return;
    }
    const labels: Record<string, string> = {
      achievements: 'Achievements',
      notifications: 'Notifications',
      privacy: 'Privacy & Security',
      subscription: 'Subscription',
      help: 'Help & Support',
    };
    showComingSoon(labels[id] ?? 'This feature');
  };

  if ((profileQuery.isLoading || progressQuery.isLoading) && !display) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator color={theme.colors.primary} size="large" />
      </View>
    );
  }

  if (!display) {
    return (
      <QueryState
        message="Could not load profile."
        onRetry={() => {
          void profileQuery.refetch();
          void progressQuery.refetch();
        }}
      />
    );
  }

  const achievements = progress ? buildAchievements(progress) : [];
  const records = progress ? buildPersonalRecords(progress) : [];

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <View style={styles.header}>
        <Text style={styles.heading}>Profile</Text>
        <Pressable
          style={styles.settingsBtn}
          onPress={() => router.push(ROUTES.editProfile)}
          hitSlop={8}
          accessibilityRole="button"
          accessibilityLabel="Edit profile"
        >
          <Settings color={theme.colors.text} size={22} />
        </Pressable>
      </View>

      <UserProfileCard user={display} onPress={() => router.push(ROUTES.editProfile)} />

      <GoalCard user={display} onEdit={() => router.push(ROUTES.editGoals)} />

      {progress ? (
        <OverviewGrid>
          <OverviewTile
            icon={Dumbbell}
            iconColor={theme.colors.primary}
            value={String(progress.totalWorkouts)}
            label="Workouts Completed"
          />
          <OverviewTile
            icon={Flame}
            iconColor={theme.colors.orange}
            value={progress.totalCaloriesBurned.toLocaleString()}
            label="Calories Burned"
          />
          <OverviewTile
            icon={Zap}
            iconColor={theme.colors.green}
            value={String(progress.currentStreak)}
            label="Day Streak"
          />
          <OverviewTile
            icon={Scale}
            iconColor={theme.colors.purple}
            value={`${display.currentWeight} kg`}
            label="Current Weight"
          />
        </OverviewGrid>
      ) : null}

      {achievements.length > 0 ? <AchievementsSection items={achievements} /> : null}

      {records.length > 0 ? <PersonalRecordsSection records={records} /> : null}

      <SettingsMenu items={menuItems} onSelect={handleMenu} />

      {profileQuery.isError ? (
        <Text style={styles.errorHint}>{getApiErrorMessage(profileQuery.error)}</Text>
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
    alignItems: 'center',
    marginBottom: theme.spacing.md,
  },
  heading: { fontSize: 28, fontWeight: '700', color: theme.colors.text },
  settingsBtn: {
    width: 40,
    height: 40,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.surface,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  errorHint: { color: theme.colors.textMuted, textAlign: 'center', fontSize: 12 },
});
