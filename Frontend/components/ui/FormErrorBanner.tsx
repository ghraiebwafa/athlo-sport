import { AlertCircle } from 'lucide-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/constants/theme';

interface FormErrorBannerProps {
  message?: string;
}

export function FormErrorBanner({ message }: FormErrorBannerProps) {
  if (!message) return null;

  return (
    <View style={styles.banner} accessibilityRole="alert">
      <AlertCircle color={theme.colors.error} size={18} />
      <Text style={styles.text}>{message}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  banner: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: 10,
    backgroundColor: 'rgba(255, 59, 48, 0.12)',
    borderWidth: 1,
    borderColor: 'rgba(255, 59, 48, 0.35)',
    borderRadius: theme.radius.md,
    paddingHorizontal: 14,
    paddingVertical: 12,
  },
  text: {
    flex: 1,
    color: theme.colors.error,
    fontSize: 14,
    lineHeight: 20,
  },
});
