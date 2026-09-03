import { router } from 'expo-router';
import { useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Pressable,
  ScrollView,
  Share,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { deleteAccount, exportAccountData } from '@/lib/api/auth';
import { getApiErrorMessage } from '@/lib/api/client';
import { signOutAndRedirect } from '@/lib/authSession';
import { ROUTES } from '@/lib/routes';
import { captureException } from '@/lib/telemetry';

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
    body: 'Workout history, goals, and personal records are tied to your account. You can export a copy or permanently delete your account below.',
  },
  {
    title: 'Permissions',
    body: 'The app only requests permissions needed for core features. Camera, location, and microphone are not required for the current workout flows.',
  },
];

export default function PrivacyScreen() {
  const [busy, setBusy] = useState<'export' | 'delete' | null>(null);
  const [password, setPassword] = useState('');

  const handleExport = async () => {
    setBusy('export');
    try {
      const data = await exportAccountData();
      await Share.share({
        message: JSON.stringify(data, null, 2),
        title: 'Athlo data export',
      });
    } catch (err) {
      captureException(err, { action: 'exportAccount' });
      Alert.alert('Export failed', getApiErrorMessage(err));
    } finally {
      setBusy(null);
    }
  };

  const confirmDelete = () => {
    Alert.alert(
      'Delete account?',
      'This permanently removes your workouts, saved programs, and profile. This cannot be undone.',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Continue',
          style: 'destructive',
          onPress: () => {
            if (!password.trim()) {
              Alert.alert('Password required', 'Enter your password to confirm deletion.');
              return;
            }
            void runDelete();
          },
        },
      ]
    );
  };

  const runDelete = async () => {
    setBusy('delete');
    try {
      await deleteAccount(password.trim());
      await signOutAndRedirect();
    } catch (err) {
      captureException(err, { action: 'deleteAccount' });
      Alert.alert('Could not delete account', getApiErrorMessage(err));
      setBusy(null);
    }
  };

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Privacy & Security" onBack={() => router.back()} />
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        {sections.map((section) => (
          <View key={section.title} style={styles.card}>
            <Text style={styles.title}>{section.title}</Text>
            <Text style={styles.body}>{section.body}</Text>
          </View>
        ))}

        <Text style={styles.link} onPress={() => router.push(ROUTES.changePassword)}>
          Change password →
        </Text>

        <View style={styles.card}>
          <Text style={styles.title}>Export your data</Text>
          <Text style={styles.body}>
            Download a JSON copy of your profile, preferences, workouts, and saved programs.
          </Text>
          <Pressable
            style={styles.secondaryBtn}
            onPress={() => void handleExport()}
            disabled={busy != null}
          >
            {busy === 'export' ? (
              <ActivityIndicator color={theme.colors.primary} />
            ) : (
              <Text style={styles.secondaryBtnText}>Export data</Text>
            )}
          </Pressable>
        </View>

        <View style={[styles.card, styles.dangerCard]}>
          <Text style={styles.title}>Delete account</Text>
          <Text style={styles.body}>
            Permanently erase your Athlo account and all associated workout data.
          </Text>
          <TextInput
            style={styles.input}
            value={password}
            onChangeText={setPassword}
            placeholder="Confirm with your password"
            placeholderTextColor={theme.colors.textMuted}
            secureTextEntry
            autoComplete="password"
          />
          <Pressable
            style={styles.dangerBtn}
            onPress={confirmDelete}
            disabled={busy != null}
          >
            {busy === 'delete' ? (
              <ActivityIndicator color="#fff" />
            ) : (
              <Text style={styles.dangerBtnText}>Delete my account</Text>
            )}
          </Pressable>
        </View>
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
  dangerCard: { borderColor: theme.colors.red },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  body: { color: theme.colors.textMuted, fontSize: 14, lineHeight: 20 },
  link: {
    color: theme.colors.primary,
    fontWeight: '600',
    textAlign: 'center',
    marginVertical: theme.spacing.sm,
  },
  secondaryBtn: {
    marginTop: theme.spacing.sm,
    borderRadius: theme.radius.md,
    borderWidth: 1,
    borderColor: theme.colors.primary,
    paddingVertical: 12,
    alignItems: 'center',
  },
  secondaryBtnText: { color: theme.colors.primary, fontWeight: '700' },
  input: {
    marginTop: theme.spacing.sm,
    borderWidth: 1,
    borderColor: theme.colors.border,
    borderRadius: theme.radius.md,
    paddingHorizontal: 12,
    paddingVertical: 10,
    color: theme.colors.text,
  },
  dangerBtn: {
    marginTop: theme.spacing.sm,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.red,
    paddingVertical: 12,
    alignItems: 'center',
  },
  dangerBtnText: { color: '#fff', fontWeight: '700' },
});
