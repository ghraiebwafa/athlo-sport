import { router } from 'expo-router';
import { useEffect } from 'react';
import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import {
  type HeartRateSource,
  REST_PRESET_SECONDS,
  type RestPresetSeconds,
  usePreferencesStore,
} from '@/stores/preferencesStore';

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
  const defaultRestSeconds = usePreferencesStore((s) => s.defaultRestSeconds);
  const betweenExerciseRestSeconds = usePreferencesStore((s) => s.betweenExerciseRestSeconds);
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

        <Text style={styles.sectionTitle}>Rest timer defaults</Text>
        <Text style={styles.intro}>
          Applied automatically when you log a set during a workout. You can still adjust rest on the fly.
        </Text>

        <RestPresetGroup
          label="Between sets"
          value={defaultRestSeconds}
          onChange={(seconds) => void setPreference('defaultRestSeconds', seconds)}
        />
        <RestPresetGroup
          label="Between exercises"
          value={betweenExerciseRestSeconds}
          onChange={(seconds) => void setPreference('betweenExerciseRestSeconds', seconds)}
        />

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

function RestPresetGroup({
  label,
  value,
  onChange,
}: {
  label: string;
  value: RestPresetSeconds;
  onChange: (seconds: RestPresetSeconds) => void;
}) {
  return (
    <View style={styles.restGroup}>
      <Text style={styles.restLabel}>{label}</Text>
      <View style={styles.restPresets}>
        {REST_PRESET_SECONDS.map((seconds) => {
          const selected = value === seconds;
          return (
            <Pressable
              key={seconds}
              style={[styles.restChip, selected && styles.restChipSelected]}
              onPress={() => onChange(seconds)}
              accessibilityRole="button"
              accessibilityState={{ selected }}
              accessibilityLabel={`${label} ${seconds} seconds`}
            >
              <Text style={[styles.restChipText, selected && styles.restChipTextSelected]}>
                {seconds}s
              </Text>
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  content: { padding: theme.spacing.md, paddingBottom: 40, gap: theme.spacing.sm },
  intro: { color: theme.colors.textMuted, marginBottom: theme.spacing.sm, lineHeight: 20 },
  sectionTitle: {
    color: theme.colors.text,
    fontWeight: '700',
    fontSize: 16,
    marginTop: theme.spacing.md,
  },
  restGroup: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
    gap: theme.spacing.sm,
  },
  restLabel: { color: theme.colors.text, fontWeight: '600', fontSize: 14 },
  restPresets: { flexDirection: 'row', gap: 8 },
  restChip: {
    flex: 1,
    paddingVertical: 10,
    borderRadius: theme.radius.md,
    backgroundColor: theme.colors.surfaceLight,
    alignItems: 'center',
  },
  restChipSelected: { backgroundColor: theme.colors.primary },
  restChipText: { color: theme.colors.textMuted, fontWeight: '700' },
  restChipTextSelected: { color: '#fff' },
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
