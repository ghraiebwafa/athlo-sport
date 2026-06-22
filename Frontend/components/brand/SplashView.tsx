import { ActivityIndicator, ImageBackground, Platform, StyleSheet, Text, View, ViewStyle } from 'react-native';
import { AthloLogo } from '@/components/brand/AthloLogo';
import { images } from '@/constants/images';
import { theme } from '@/constants/theme';

export function SplashView() {
  return (
    <ImageBackground
      source={images.bgSplash}
      style={[styles.background, Platform.OS === 'web' && styles.backgroundWeb]}
      imageStyle={styles.backgroundImage}
      resizeMode="cover"
    >
      <View style={styles.overlay} />
      <View style={styles.content}>
        <AthloLogo size="xl" />
      </View>
      <View style={styles.footer}>
        <ActivityIndicator color={theme.colors.primary} size="small" />
        <Text style={styles.loadingText}>Loading your best self...</Text>
      </View>
    </ImageBackground>
  );
}

const styles = StyleSheet.create({
  background: { flex: 1 },
  backgroundWeb: {
    width: '100%',
    minHeight: '100vh',
  } as unknown as ViewStyle,
  backgroundImage: {
    width: '100%',
    height: '100%',
  },
  overlay: {
    ...StyleSheet.absoluteFill,
    backgroundColor: 'rgba(0,0,0,0.55)',
  },
  content: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: theme.spacing.lg,
  },
  footer: {
    alignItems: 'center',
    paddingBottom: 56,
    gap: theme.spacing.md,
  },
  loadingText: {
    color: theme.colors.textMuted,
    fontSize: 14,
  },
});
