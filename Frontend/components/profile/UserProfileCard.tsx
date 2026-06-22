import { ChevronRight, Crown, LucideIcon } from 'lucide-react-native';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';
import { memberLabel } from '@/lib/profileHelpers';
import type { UserProfile } from '@/lib/types';

interface UserProfileCardProps {
  user: UserProfile;
  onPress?: () => void;
}

export function UserProfileCard({ user, onPress }: UserProfileCardProps) {
  const initial = user.fullName.charAt(0).toUpperCase();

  return (
    <Pressable style={({ pressed }) => [styles.card, pressed && styles.pressed]} onPress={onPress}>
      <View style={styles.avatarWrap}>
        <View style={styles.avatar}>
          <Text style={styles.initial}>{initial}</Text>
        </View>
      </View>
      <View style={styles.info}>
        <Text style={styles.name}>{user.fullName}</Text>
        <View style={styles.badgeRow}>
          <Crown color={theme.colors.primary} size={12} />
          <Text style={styles.badge}>{memberLabel(user.role)}</Text>
        </View>
        <Text style={styles.email}>{user.email}</Text>
      </View>
      <ChevronRight color={theme.colors.textMuted} size={22} />
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginBottom: theme.spacing.md,
    gap: theme.spacing.md,
  },
  pressed: { opacity: 0.92 },
  avatarWrap: { position: 'relative' },
  avatar: {
    width: 56,
    height: 56,
    borderRadius: 28,
    backgroundColor: theme.colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  initial: { color: '#fff', fontSize: 24, fontWeight: '700' },
  info: { flex: 1 },
  name: { color: theme.colors.text, fontSize: 18, fontWeight: '700' },
  badgeRow: { flexDirection: 'row', alignItems: 'center', gap: 4, marginTop: 4 },
  badge: { color: theme.colors.primary, fontSize: 12, fontWeight: '600' },
  email: { color: theme.colors.textMuted, fontSize: 13, marginTop: 4 },
});
