import { Bookmark, Play } from 'lucide-react-native';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface ProgramActionsProps {
  onStart: () => void;
  onSave: () => void;
  starting?: boolean;
  saved: boolean;
}

export function ProgramActions({ onStart, onSave, starting, saved }: ProgramActionsProps) {
  return (
    <View style={styles.wrap}>
      <Pressable
        style={({ pressed }) => [styles.primary, pressed && styles.pressed, starting && styles.disabled]}
        onPress={onStart}
        disabled={starting}
      >
        {starting ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <>
            <Play color="#fff" size={20} fill="#fff" />
            <Text style={styles.primaryText}>Start Workout</Text>
          </>
        )}
      </Pressable>
      <Pressable style={({ pressed }) => [styles.secondary, pressed && styles.pressed]} onPress={onSave}>
        <Bookmark color={theme.colors.text} size={18} fill={saved ? theme.colors.text : 'transparent'} />
        <Text style={styles.secondaryText}>{saved ? 'Saved' : 'Save for Later'}</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: { gap: theme.spacing.sm, marginTop: theme.spacing.lg },
  primary: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 10,
    backgroundColor: theme.colors.primary,
    borderRadius: theme.radius.lg,
    paddingVertical: 16,
  },
  primaryText: { color: '#fff', fontSize: 16, fontWeight: '700' },
  secondary: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 10,
    borderRadius: theme.radius.lg,
    paddingVertical: 16,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  secondaryText: { color: theme.colors.text, fontSize: 16, fontWeight: '600' },
  pressed: { opacity: 0.85 },
  disabled: { opacity: 0.6 },
});
