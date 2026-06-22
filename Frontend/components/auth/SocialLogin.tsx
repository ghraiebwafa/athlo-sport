import { Alert, Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

const providers = [
  { id: 'apple', label: 'Apple', glyph: '\uF8FF' },
  { id: 'google', label: 'Google', glyph: 'G' },
  { id: 'facebook', label: 'Facebook', glyph: 'f' },
] as const;

export function SocialLogin() {
  const onPress = (label: string) => {
    Alert.alert('Coming soon', `${label} sign-in is not available yet.`);
  };

  return (
    <View style={styles.wrap}>
      <View style={styles.dividerRow}>
        <View style={styles.line} />
        <Text style={styles.dividerText}>or continue with</Text>
        <View style={styles.line} />
      </View>
      <View style={styles.row}>
        {providers.map((p) => (
          <Pressable key={p.id} style={styles.tile} onPress={() => onPress(p.label)}>
            <Text style={[styles.glyph, p.id === 'facebook' && styles.facebook]}>{p.glyph}</Text>
            <Text style={styles.tileLabel}>{p.label}</Text>
          </Pressable>
        ))}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: { gap: theme.spacing.md },
  dividerRow: { flexDirection: 'row', alignItems: 'center', gap: 12 },
  line: { flex: 1, height: 1, backgroundColor: theme.colors.border },
  dividerText: { color: theme.colors.textMuted, fontSize: 13 },
  row: { flexDirection: 'row', gap: theme.spacing.sm },
  tile: {
    flex: 1,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    paddingVertical: 14,
    alignItems: 'center',
    gap: 6,
  },
  glyph: { fontSize: 22, color: theme.colors.text, fontWeight: '600' },
  facebook: { color: theme.colors.primary },
  tileLabel: { color: theme.colors.textMuted, fontSize: 12 },
});
