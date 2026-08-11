import { router } from 'expo-router';
import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { ROUTES } from '@/lib/routes';

const sections = [
  {
    title: 'Account security',
    body: 'Use a strong unique password and change it regularly. You can update it anytime from Change Password in your profile.',
  },
  {
    title: 'Session & tokens',
    body: 'Athlo keeps you signed in with short-lived access tokens and rotating refresh tokens. Signing out revokes your refresh token on the server.',
  },
  {
    title: 'Your data',
    body: 'Workout history, goals, and personal records are tied to your account. Only you (and admins when required for support) can access that data through the API.',
  },
  {
    title: 'Permissions',
    body: 'The app only requests permissions needed for core features. Camera, location, and microphone are not required for the current workout flows.',
  },
];

export default function PrivacyScreen() {
  return (
    <View style={styles.screen}>
      <ScreenHeader title="Privacy & Security" onBack={() => router.back()} />
      <ScrollView contentContainerStyle={styles.content}>
        {sections.map((section) => (
          <View key={section.title} style={styles.card}>
            <Text style={styles.title}>{section.title}</Text>
            <Text style={styles.body}>{section.body}</Text>
          </View>
        ))}
        <Text style={styles.link} onPress={() => router.push(ROUTES.changePassword)}>
          Change password →
        </Text>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  content: { padding: theme.spacing.md, paddingBottom: 40, gap: theme.spacing.sm },
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
    gap: 6,
  },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  body: { color: theme.colors.textMuted, fontSize: 14, lineHeight: 20 },
  link: {
    color: theme.colors.primary,
    fontWeight: '600',
    textAlign: 'center',
    marginTop: theme.spacing.md,
  },
});
