import { router } from 'expo-router';
import { useEffect } from 'react';
import { Alert, Pressable, ScrollView, StyleSheet, Switch, Text, View } from 'react-native';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { enablePushNotifications } from '@/lib/pushNotifications';
import { usePreferencesStore } from '@/stores/preferencesStore';

export default function NotificationsScreen() {
  const hydrated = usePreferencesStore((s) => s.hydrated);
  const hydrate = usePreferencesStore((s) => s.hydrate);
  const notifyWorkoutReminders = usePreferencesStore((s) => s.notifyWorkoutReminders);
  const notifyPrAlerts = usePreferencesStore((s) => s.notifyPrAlerts);
  const notifyStreakReminders = usePreferencesStore((s) => s.notifyStreakReminders);
  const pushPermissionAsked = usePreferencesStore((s) => s.pushPermissionAsked);
  const setPreference = usePreferencesStore((s) => s.setPreference);

  useEffect(() => {
    if (!hydrated) void hydrate();
  }, [hydrated, hydrate]);

  const requestPermission = async () => {
    await setPreference('pushPermissionAsked', true);
    const token = await enablePushNotifications();
    if (token) {
      Alert.alert('Notifications enabled', 'You will receive reminders based on your preferences.');
      return;
    }
    Alert.alert(
      'Could not enable push',
      'Permission was denied, or this build cannot register for push. Your notification preferences are still saved.'
    );
  };

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Notifications" onBack={() => router.back()} />
      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.intro}>
          Choose what Athlo should remind you about. Preferences sync with your account when you are signed in.
        </Text>

        <PrefRow
          label="Workout reminders"
          hint="Nudge you when you have not trained in a while"
          value={notifyWorkoutReminders}
          onChange={(v) => void setPreference('notifyWorkoutReminders', v)}
        />
        <PrefRow
          label="Personal record alerts"
          hint="Celebrate new lift PRs"
          value={notifyPrAlerts}
          onChange={(v) => void setPreference('notifyPrAlerts', v)}
        />
        <PrefRow
          label="Streak reminders"
          hint="Help protect your current streak"
          value={notifyStreakReminders}
          onChange={(v) => void setPreference('notifyStreakReminders', v)}
        />

        <Pressable style={styles.cta} onPress={() => void requestPermission()}>
          <Text style={styles.ctaText}>
            {pushPermissionAsked ? 'Re-enable device notifications' : 'Enable device notifications'}
          </Text>
        </Pressable>
        <Text style={styles.note}>
          On a physical device with a development or production build, Athlo registers an Expo push token with the
          server so reminders and achievements can reach you.
        </Text>
      </ScrollView>
    </View>
  );
}

function PrefRow({
  label,
  hint,
  value,
  onChange,
}: {
  label: string;
  hint: string;
  value: boolean;
  onChange: (value: boolean) => void;
}) {
  return (
    <View style={styles.row}>
      <View style={styles.rowBody}>
        <Text style={styles.label}>{label}</Text>
        <Text style={styles.hint}>{hint}</Text>
      </View>
      <Switch
        value={value}
        onValueChange={onChange}
        trackColor={{ false: theme.colors.surfaceLight, true: `${theme.colors.primary}88` }}
        thumbColor={value ? theme.colors.primary : theme.colors.textMuted}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  content: { padding: theme.spacing.md, paddingBottom: 40, gap: theme.spacing.sm },
  intro: { color: theme.colors.textMuted, marginBottom: theme.spacing.sm, lineHeight: 20 },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
  },
  rowBody: { flex: 1 },
  label: { color: theme.colors.text, fontWeight: '600', fontSize: 15 },
  hint: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
  cta: {
    marginTop: theme.spacing.md,
    backgroundColor: theme.colors.primary,
    borderRadius: theme.radius.lg,
    paddingVertical: 14,
    alignItems: 'center',
  },
  ctaText: { color: '#fff', fontWeight: '700' },
  note: { color: theme.colors.textMuted, fontSize: 12, lineHeight: 18, textAlign: 'center' },
});
