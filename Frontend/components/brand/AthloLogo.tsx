import { Image, StyleSheet, Text, View } from 'react-native';
import { brand } from '@/constants/brand';
import { images } from '@/constants/images';
import { theme } from '@/constants/theme';

interface AthloLogoProps {
  size?: 'sm' | 'md' | 'lg' | 'xl';
  layout?: 'stacked' | 'horizontal';
  showTagline?: boolean;
  centered?: boolean;
}

/** Height ratio of logo.png that contains only the blue A mark (excludes baked-in text). */
const ICON_HEIGHT_RATIO = 0.46;

const iconWidths = { sm: 88, md: 112, lg: 140, xl: 168 } as const;
const nameSizes = { sm: 32, md: 40, lg: 48, xl: 56 } as const;
const horizontalNameSizes = { sm: 28, md: 34, lg: 40, xl: 44 } as const;
const taglineSizes = { sm: 9, md: 10, lg: 11, xl: 12 } as const;

function LogoIcon({ width }: { width: number }) {
  const clipHeight = width * ICON_HEIGHT_RATIO;
  return (
    <View style={[styles.iconClip, { width, height: clipHeight }]}>
      <Image
        source={images.logo}
        style={{ width, height: width * 0.92 }}
        resizeMode="contain"
        accessibilityLabel={`${brand.name} icon`}
      />
    </View>
  );
}

/** ATHLO logo — stacked (splash/auth) or horizontal (onboarding), matching mockups. */
export function AthloLogo({
  size = 'md',
  layout = 'stacked',
  showTagline = true,
  centered = true,
}: AthloLogoProps) {
  const iconWidth = iconWidths[size];

  if (layout === 'horizontal') {
    return (
      <View style={[styles.root, centered && styles.centered]}>
        <View style={styles.horizontalRow}>
          <LogoIcon width={iconWidth * 0.55} />
          <Text style={[styles.name, { fontSize: horizontalNameSizes[size] }]}>{brand.name}</Text>
        </View>
      </View>
    );
  }

  return (
    <View style={[styles.root, centered && styles.centered]}>
      <LogoIcon width={iconWidth} />
      <Text style={[styles.name, { fontSize: nameSizes[size] }]}>{brand.name}</Text>
      {showTagline ? (
        <Text style={[styles.tagline, { fontSize: taglineSizes[size] }]}>{brand.tagline}</Text>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  root: { alignItems: 'center', gap: 10 },
  centered: { alignSelf: 'center' },
  horizontalRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
  },
  iconClip: { overflow: 'hidden', alignItems: 'center' },
  name: {
    color: theme.colors.text,
    fontWeight: '800',
    letterSpacing: 4,
  },
  tagline: {
    color: theme.colors.textMuted,
    fontWeight: '500',
    letterSpacing: 2.5,
    textTransform: 'uppercase',
    marginTop: 2,
  },
});
