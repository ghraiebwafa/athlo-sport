import { router } from 'expo-router';
import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { ROUTES } from '@/lib/routes';

const faqs = [
  {
    q: 'How do I start a workout?',
    a: 'Open Programs, pick a plan, then start the session. Log each set with reps and optional weight as you go.',
  },
  {
    q: 'I still have an active workout.',
    a: 'Return from Home or finish/discard it from the active workout screen. Abandoned sessions are cleared automatically after 24 hours.',
  },
  {
    q: 'Where are my personal records?',
    a: 'PRs come from completed weighted sets. They appear on your Profile once you log weights during workouts.',
  },
  {
    q: 'How do I change goals or password?',
    a: 'Use Edit Goals or Change Password from the profile menu.',
  },
];

export default function HelpScreen() {
  return (
    <View style={styles.screen}>
      <ScreenHeader title="Help & Support" onBack={() => router.back()} />
      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.intro}>Quick answers for common Athlo questions.</Text>
        {faqs.map((item) => (
          <View key={item.q} style={styles.card}>
            <Text style={styles.q}>{item.q}</Text>
            <Text style={styles.a}>{item.a}</Text>
          </View>
        ))}
        <Text style={styles.link} onPress={() => router.push(ROUTES.programs)}>
          Browse programs →
        </Text>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  content: { padding: theme.spacing.md, paddingBottom: 40, gap: theme.spacing.sm },
  intro: { color: theme.colors.textMuted, marginBottom: theme.spacing.sm },
  card: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
    gap: 6,
  },
  q: { color: theme.colors.text, fontWeight: '700', fontSize: 15 },
  a: { color: theme.colors.textMuted, fontSize: 14, lineHeight: 20 },
  link: {
    color: theme.colors.primary,
    fontWeight: '600',
    textAlign: 'center',
    marginTop: theme.spacing.md,
  },
});
