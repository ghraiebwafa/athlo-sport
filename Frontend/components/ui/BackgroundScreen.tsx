import {
  ImageBackground,
  ImageSourcePropType,
  Platform,
  ScrollView,
  StyleProp,
  StyleSheet,
  View,
  ViewStyle,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ReactNode } from 'react';
import { webPhoneFrame } from '@/lib/layout';

interface BackgroundScreenProps {
  source: ImageSourcePropType;
  children: ReactNode;
  overlayOpacity?: number;
  scroll?: boolean;
  contentStyle?: StyleProp<ViewStyle>;
  /** Center content in a phone-width column on web; background stays full-screen. */
  phoneFrame?: boolean;
}

export function BackgroundScreen({
  source,
  children,
  overlayOpacity = 0.62,
  scroll,
  contentStyle,
  phoneFrame = true,
}: BackgroundScreenProps) {
  const framedChildren = phoneFrame ? (
    <View style={[styles.phoneFrame, webPhoneFrame]}>{children}</View>
  ) : (
    children
  );

  const body = scroll ? (
    <ScrollView
      contentContainerStyle={[styles.scrollContent, contentStyle]}
      keyboardShouldPersistTaps="handled"
      showsVerticalScrollIndicator={false}
    >
      {framedChildren}
    </ScrollView>
  ) : (
    <View style={[styles.content, contentStyle]}>{framedChildren}</View>
  );

  return (
    <ImageBackground
      source={source}
      style={[styles.background, Platform.OS === 'web' && styles.backgroundWeb]}
      imageStyle={styles.backgroundImage}
      resizeMode="cover"
    >
      <View style={[styles.overlay, { backgroundColor: `rgba(0,0,0,${overlayOpacity})` }]} />
      <SafeAreaView style={styles.safe}>{body}</SafeAreaView>
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
  overlay: { ...StyleSheet.absoluteFill },
  safe: { flex: 1 },
  content: { flex: 1 },
  scrollContent: {
    flexGrow: 1,
    width: '100%',
    alignItems: 'center',
  },
  phoneFrame: {
    width: '100%',
    flexGrow: 1,
  },
});
