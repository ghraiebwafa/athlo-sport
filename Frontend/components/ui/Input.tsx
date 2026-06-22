import { Eye, EyeOff, LucideIcon } from 'lucide-react-native';
import { useState } from 'react';
import { Pressable, StyleSheet, Text, TextInput, TextInputProps, View } from 'react-native';
import { theme } from '@/constants/theme';

interface InputProps extends TextInputProps {
  label?: string;
  error?: string;
  icon?: LucideIcon;
  secureToggle?: boolean;
}

export function Input({ label, error, icon: Icon, secureToggle, secureTextEntry, style, ...props }: InputProps) {
  const [hidden, setHidden] = useState(!!secureTextEntry);

  return (
    <View style={styles.wrapper}>
      {label ? <Text style={styles.label}>{label}</Text> : null}
      <View style={[styles.field, error && styles.fieldError]}>
        {Icon ? (
          <Icon color={theme.colors.textMuted} size={18} style={styles.icon} />
        ) : null}
        <TextInput
          placeholderTextColor={theme.colors.textMuted}
          style={[styles.input, style]}
          secureTextEntry={secureToggle ? hidden : secureTextEntry}
          {...props}
        />
        {secureToggle ? (
          <Pressable onPress={() => setHidden((v) => !v)} hitSlop={8}>
            {hidden ? (
              <EyeOff color={theme.colors.textMuted} size={18} />
            ) : (
              <Eye color={theme.colors.textMuted} size={18} />
            )}
          </Pressable>
        ) : null}
      </View>
      {error ? <Text style={styles.error}>{error}</Text> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  wrapper: { gap: 6 },
  label: { color: theme.colors.textMuted, fontSize: 14, fontWeight: '500' },
  field: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: theme.colors.surface,
    borderWidth: 1,
    borderColor: theme.colors.surfaceLight,
    borderRadius: theme.radius.lg,
    paddingHorizontal: 14,
  },
  fieldError: { borderColor: theme.colors.error },
  icon: { marginRight: 10 },
  input: {
    flex: 1,
    paddingVertical: 14,
    color: theme.colors.text,
    fontSize: 16,
  },
  error: { color: theme.colors.error, fontSize: 12 },
});
