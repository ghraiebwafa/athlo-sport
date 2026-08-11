import { StyleSheet, Text, View } from 'react-native';
import Svg, { Circle } from 'react-native-svg';
import { theme } from '@/constants/theme';

interface MuscleSlice {
  name: string;
  color: string;
  percent: number;
}

interface MuscleGroupChartProps {
  data: MuscleSlice[];
  mostTrained: string;
}

export function MuscleGroupChart({ data, mostTrained }: MuscleGroupChartProps) {
  const size = 120;
  const stroke = 18;
  const radius = (size - stroke) / 2;
  const circumference = 2 * Math.PI * radius;

  const slices = data.reduce<
    { name: string; color: string; percent: number; dash: number; offset: number }[]
  >((acc, slice) => {
    const dash = (slice.percent / 100) * circumference;
    const offset = acc.reduce((sum, s) => sum + s.dash, 0);
    acc.push({ ...slice, dash, offset });
    return acc;
  }, []);

  return (
    <View style={styles.wrap}>
      <View style={styles.header}>
        <View>
          <Text style={styles.title}>Muscle Group Focus</Text>
          <Text style={styles.estimate}>Estimated from workout names</Text>
        </View>
      </View>
      <View style={styles.body}>
        <View style={styles.chartWrap}>
          <Svg width={size} height={size}>
            <Circle
              cx={size / 2}
              cy={size / 2}
              r={radius}
              stroke={theme.colors.surfaceLight}
              strokeWidth={stroke}
              fill="none"
            />
            {slices.map((slice) => (
              <Circle
                key={slice.name}
                cx={size / 2}
                cy={size / 2}
                r={radius}
                stroke={slice.color}
                strokeWidth={stroke}
                fill="none"
                strokeDasharray={`${slice.dash} ${circumference - slice.dash}`}
                strokeDashoffset={-slice.offset}
                rotation="-90"
                origin={`${size / 2}, ${size / 2}`}
              />
            ))}
          </Svg>
          <View style={styles.centerLabel}>
            <Text style={styles.centerSmall}>Most Trained</Text>
            <Text style={styles.centerValue}>{mostTrained}</Text>
          </View>
        </View>
        <View style={styles.legend}>
          {data.map((slice) => (
            <View key={slice.name} style={styles.legendRow}>
              <View style={[styles.dot, { backgroundColor: slice.color }]} />
              <Text style={styles.legendName}>{slice.name}</Text>
              <Text style={styles.legendPct}>{slice.percent}%</Text>
            </View>
          ))}
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.md,
    borderWidth: 1,
    borderColor: theme.colors.border,
    marginBottom: theme.spacing.lg,
  },
  header: { marginBottom: theme.spacing.md },
  title: { color: theme.colors.text, fontWeight: '700', fontSize: 16 },
  estimate: { color: theme.colors.textMuted, fontSize: 11, fontWeight: '500' },
  body: { flexDirection: 'row', gap: theme.spacing.md, alignItems: 'center' },
  chartWrap: { width: 120, height: 120, alignItems: 'center', justifyContent: 'center' },
  centerLabel: { position: 'absolute', alignItems: 'center', width: 90 },
  centerSmall: { color: theme.colors.textMuted, fontSize: 9 },
  centerValue: { color: theme.colors.primary, fontWeight: '700', fontSize: 14, textAlign: 'center' },
  legend: { flex: 1, gap: 8 },
  legendRow: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  dot: { width: 8, height: 8, borderRadius: 4 },
  legendName: { flex: 1, color: theme.colors.text, fontSize: 13 },
  legendPct: { color: theme.colors.textMuted, fontSize: 12 },
});
