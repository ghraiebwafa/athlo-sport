import { router } from 'expo-router';
import { useEffect } from 'react';
import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { type HeartRateSource, usePreferencesStore } from '@/stores/preferencesStore';

const options: { id: HeartRateSource; title: string; body: string }[] = [
  {
    id: 'estimated',
    title: 'Estimated HR',
    body: 'Use Athlo’s workout timer model. Works everywhere, no wearable required.',
  },
  {
    id: 'manual',
    title: 'Manual entry',
    body: 'Enter BPM yourself during a session when you have a watch or chest strap reading.',
  },
];

export default function DevicesScreen() {
  const hydrated = usePreferencesStore((s) => s.hydrated);
  const hydrate = usePreferencesStore((s) => s.hydrate);
  const heartRateSource = usePreferencesStore((s) => s.heartRateSource);
  const setPreference = usePreferencesStore((s) => s.setPreference);

  useEffect(() => {
    if (!hydrated) void hydrate();
  }, [hydrated, hydrate]);

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Devices & Heart Rate" onBack={() => router.back()} />
      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.intro}>
          Choose how Athlo should get heart-rate data during workouts. Apple Health / Google Fit can
          plug into this preference later when a native build is available.
        </Text>

        {options.map((option) => {
          const selected = heartRateSource === option.id;
          return (
            <Pressable
              key={option.id}
              style={[styles.card, selected && styles.cardSelected]}
              onPress={() => void setPreference('heartRateSource', option.id)}
              accessibilityRole="radio"
              accessibilityState={{ selected }}
            >
              <View style={[styles.radio, selected && styles.radioOn]} />
              <View style={styles.body}>
                <Text style={styles.title}>{option.title}</Text>
                <Text style={styles.hint}>{option.body}</Text>
              </View>
            </Pressable>
          );
        })}

        <View style={styles.card}>
          <Text style={styles.title}>Wearables</Text>
          <Text style={styles.hint}>
            HealthKit / Health Connect sync is scaffolded here. No vendor SDK is bundled yet so local
            development stays simple.
          </Text>
        </View>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  content: { padding: theme.spacing.md, paddingBottom: 40, gap: theme.spacing.sm },
  intro: { color: theme.colors.textMuted, marginBottom: theme.spacing.sm, lineHeight: 20 },
  card: {
    flexDirection: 'row',
    gap: theme.spacing.md,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
  },
  cardSelected: { borderColor: theme.colors.primary },
  radio: {
    width: 20,
    height: 20,
    borderRadius: 10,
    borderWidth: 2,
    borderColor: theme.colors.border,
    marginTop: 2,
  },
  radioOn: { borderColor: theme.colors.primary, backgroundColor: theme.colors.primary },
  body: { flex: 1 },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 15 },
  hint: { color: theme.colors.textMuted, fontSize: 13, marginTop: 4, lineHeight: 18 },
});
