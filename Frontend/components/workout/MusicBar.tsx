import { Music, Pause } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

export function MusicBar() {
  return (
    <View style={styles.bar}>
      <View style={styles.iconWrap}>
        <Music color={theme.colors.primary} size={18} />
      </View>
      <View style={styles.text}>
        <Text style={styles.title}>Power Workout Mix</Text>
        <Text style={styles.subtitle}>Energizing Background Music</Text>
      </View>
      <Pause color={theme.colors.primary} size={20} />
    </View>
  );
}

const styles = StyleSheet.create({
  bar: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.sm,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.sm,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginTop: theme.spacing.lg,
  },
  iconWrap: {
    width: 36,
    height: 36,
    borderRadius: theme.radius.sm,
    backgroundColor: theme.colors.surfaceLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  text: { flex: 1 },
  title: { color: theme.colors.text, fontWeight: '600', fontSize: 13 },
  subtitle: { color: theme.colors.textMuted, fontSize: 11, marginTop: 2 },
});
