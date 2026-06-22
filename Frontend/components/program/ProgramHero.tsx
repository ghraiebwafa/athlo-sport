import { router } from 'expo-router';
import { Bookmark, ChevronLeft, Dumbbell, Share2 } from 'lucide-react-native';
import { Alert, Image, Pressable, Share, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { theme } from '@/constants/theme';

interface ProgramHeroProps {
  title: string;
  imageUrl?: string;
  saved: boolean;
  onToggleSave: () => void;
}

export function ProgramHero({ title, imageUrl, saved, onToggleSave }: ProgramHeroProps) {
  const insets = useSafeAreaInsets();

  const handleShare = async () => {
    try {
      await Share.share({ message: `Check out ${title} on ATHLO!` });
    } catch {
      Alert.alert('Share', 'Unable to share right now.');
    }
  };

  return (
    <View style={styles.wrap}>
      {imageUrl ? (
        <Image source={{ uri: imageUrl }} style={styles.image} />
      ) : (
        <View style={styles.placeholder}>
          <Dumbbell color={theme.colors.primary} size={64} />
        </View>
      )}
      <View style={styles.overlay} />

      <View style={[styles.toolbar, { paddingTop: insets.top + 8 }]}>
        <Pressable style={styles.iconBtn} onPress={() => router.back()} hitSlop={8}>
          <ChevronLeft color="#fff" size={26} />
        </Pressable>
        <View style={styles.toolbarRight}>
          <Pressable style={styles.iconBtn} onPress={onToggleSave} hitSlop={8}>
            <Bookmark color="#fff" size={22} fill={saved ? '#fff' : 'transparent'} />
          </Pressable>
          <Pressable style={styles.iconBtn} onPress={handleShare} hitSlop={8}>
            <Share2 color="#fff" size={22} />
          </Pressable>
        </View>
      </View>

      <Text style={styles.title}>{title}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: { height: 280, backgroundColor: theme.colors.surfaceLight },
  image: { ...StyleSheet.absoluteFill, width: '100%', height: '100%' },
  placeholder: {
    ...StyleSheet.absoluteFill,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: theme.colors.surfaceLight,
  },
  overlay: {
    ...StyleSheet.absoluteFill,
    backgroundColor: 'rgba(0,0,0,0.45)',
  },
  toolbar: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.md,
  },
  toolbarRight: { flexDirection: 'row', gap: 8 },
  iconBtn: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(0,0,0,0.35)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  title: {
    position: 'absolute',
    bottom: theme.spacing.lg,
    left: theme.spacing.md,
    right: theme.spacing.md,
    color: '#fff',
    fontSize: 28,
    fontWeight: '800',
  },
});
