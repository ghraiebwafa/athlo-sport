import { Lock, Pause, Play, Volume2 } from 'lucide-react-native';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface WorkoutControlsProps {
  paused: boolean;
  onTogglePause: () => void;
}

export function WorkoutControls({ paused, onTogglePause }: WorkoutControlsProps) {
  return (
    <View style={styles.row}>
      <Pressable style={styles.secondary}>
        <Lock color={theme.colors.text} size={22} />
        <Text style={styles.label}>Lock</Text>
      </Pressable>
      <Pressable style={styles.primary} onPress={onTogglePause}>
        {paused ? (
          <Play color="#fff" size={28} fill="#fff" />
        ) : (
          <Pause color="#fff" size={28} fill="#fff" />
        )}
        <Text style={styles.primaryLabel}>{paused ? 'Resume' : 'Pause'}</Text>
      </Pressable>
      <Pressable style={styles.secondary}>
        <Volume2 color={theme.colors.text} size={22} />
        <Text style={styles.label}>Audio</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  row: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: theme.spacing.lg },
  secondary: { alignItems: 'center', gap: 6, width: 64 },
  label: { color: theme.colors.textMuted, fontSize: 11 },
  primary: {
    width: 72,
    height: 72,
    borderRadius: 36,
    backgroundColor: theme.colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  primaryLabel: { display: 'none' },
});
