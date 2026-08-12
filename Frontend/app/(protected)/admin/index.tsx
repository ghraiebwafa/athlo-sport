import { useQuery } from '@tanstack/react-query';
import { router } from 'expo-router';
import {
  Dumbbell,
  FolderTree,
  LayoutDashboard,
  ListChecks,
  Shield,
  Users,
} from 'lucide-react-native';
import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { AdminGuard } from '@/components/auth/AdminGuard';
import { QueryState } from '@/components/ui/QueryState';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { getAdminStats } from '@/lib/api/admin';
import { getApiErrorMessage } from '@/lib/api/client';
import { ROUTES } from '@/lib/routes';
import { isSuperAdminRole } from '@/lib/roles';
import { useAuthStore } from '@/stores/authStore';

const tiles = [
  { id: 'programs', label: 'Programs', hint: 'Create and edit workout plans', icon: ListChecks, route: ROUTES.adminPrograms },
  { id: 'exercises', label: 'Exercises', hint: 'Manage exercise catalog', icon: Dumbbell, route: ROUTES.adminExercises },
  { id: 'categories', label: 'Categories', hint: 'Organize programs', icon: FolderTree, route: ROUTES.adminCategories },
  { id: 'users', label: 'Users', hint: 'Browse registered users', icon: Users, route: ROUTES.adminUsers },
] as const;

export default function AdminHubScreen() {
  const user = useAuthStore((s) => s.user);
  const isSuperAdmin = isSuperAdminRole(user?.role);

  const statsQuery = useQuery({
    queryKey: ['adminStats'],
    queryFn: getAdminStats,
    enabled: isSuperAdmin,
  });

  return (
    <AdminGuard>
      <View style={styles.screen}>
        <ScreenHeader title="Admin" onBack={() => router.replace(ROUTES.profile)} />
        <ScrollView contentContainerStyle={styles.content}>
          <Text style={styles.subtitle}>Catalog and user management</Text>

          {isSuperAdmin ? (
            <View style={styles.statsCard}>
              <View style={styles.statsHeader}>
                <LayoutDashboard color={theme.colors.primary} size={18} />
                <Text style={styles.statsTitle}>Dashboard</Text>
              </View>
              {statsQuery.isError ? (
                <QueryState
                  message={getApiErrorMessage(statsQuery.error)}
                  onRetry={() => void statsQuery.refetch()}
                />
              ) : (
                <View style={styles.statsGrid}>
                  <Stat label="Users" value={statsQuery.data?.totalUsers} />
                  <Stat label="Admins" value={statsQuery.data?.totalAdmins} />
                  <Stat label="Programs" value={statsQuery.data?.totalPrograms} />
                  <Stat label="Exercises" value={statsQuery.data?.totalExercises} />
                  <Stat label="Done today" value={statsQuery.data?.completedWorkoutsToday} />
                  <Stat label="Active now" value={statsQuery.data?.activeWorkoutsNow} />
                </View>
              )}
            </View>
          ) : null}

          {isSuperAdmin ? (
            <Pressable
              style={({ pressed }) => [styles.tile, pressed && styles.pressed]}
              onPress={() => router.push(ROUTES.adminAdmins)}
              accessibilityRole="button"
              accessibilityLabel="Manage admins"
            >
              <View style={styles.tileIcon}>
                <Shield color={theme.colors.primary} size={22} />
              </View>
              <View style={styles.tileBody}>
                <Text style={styles.tileLabel}>Admins</Text>
                <Text style={styles.tileHint}>Create and remove admin accounts</Text>
              </View>
            </Pressable>
          ) : null}

          {tiles.map((tile) => {
            const Icon = tile.icon;
            return (
              <Pressable
                key={tile.id}
                style={({ pressed }) => [styles.tile, pressed && styles.pressed]}
                onPress={() => router.push(tile.route)}
                accessibilityRole="button"
                accessibilityLabel={tile.label}
              >
                <View style={styles.tileIcon}>
                  <Icon color={theme.colors.primary} size={22} />
                </View>
                <View style={styles.tileBody}>
                  <Text style={styles.tileLabel}>{tile.label}</Text>
                  <Text style={styles.tileHint}>{tile.hint}</Text>
                </View>
              </Pressable>
            );
          })}
        </ScrollView>
      </View>
    </AdminGuard>
  );
}

function Stat({ label, value }: { label: string; value?: number }) {
  return (
    <View style={styles.stat}>
      <Text style={styles.statValue}>{value ?? '—'}</Text>
      <Text style={styles.statLabel}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  content: { padding: theme.spacing.md, paddingBottom: theme.spacing.xl, gap: theme.spacing.sm },
  subtitle: { color: theme.colors.textMuted, marginBottom: theme.spacing.sm },
  statsCard: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
    marginBottom: theme.spacing.sm,
  },
  statsHeader: { flexDirection: 'row', alignItems: 'center', gap: 8, marginBottom: theme.spacing.sm },
  statsTitle: { color: theme.colors.text, fontWeight: '700' },
  statsGrid: { flexDirection: 'row', flexWrap: 'wrap' },
  stat: { width: '33.33%', paddingVertical: 8 },
  statValue: { color: theme.colors.text, fontSize: 20, fontWeight: '700' },
  statLabel: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
  tile: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
  },
  pressed: { opacity: 0.85 },
  tileIcon: {
    width: 44,
    height: 44,
    borderRadius: theme.radius.md,
    backgroundColor: `${theme.colors.primary}22`,
    alignItems: 'center',
    justifyContent: 'center',
  },
  tileBody: { flex: 1 },
  tileLabel: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  tileHint: { color: theme.colors.textMuted, fontSize: 13, marginTop: 2 },
});
