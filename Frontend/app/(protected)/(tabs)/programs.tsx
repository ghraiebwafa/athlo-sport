import { useQuery } from '@tanstack/react-query';
import { router } from 'expo-router';
import { Bell } from 'lucide-react-native';
import { useMemo, useState } from 'react';
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { CategoryCard } from '@/components/programs/CategoryCard';
import { FeaturedProgramCard } from '@/components/programs/FeaturedProgramCard';
import { PopularWorkoutRow } from '@/components/programs/PopularWorkoutRow';
import { SearchBar } from '@/components/programs/SearchBar';
import { theme } from '@/constants/theme';
import { ROUTES } from '@/lib/routes';
import { getApiErrorMessage } from '@/lib/api/client';
import { getCategories, getPrograms } from '@/lib/api/programs';
import { getActiveWorkout } from '@/lib/api/workouts';

export default function ProgramsScreen() {
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState<string | null>(null);

  const programsQuery = useQuery({ queryKey: ['programs'], queryFn: getPrograms });
  const categoriesQuery = useQuery({ queryKey: ['categories'], queryFn: getCategories });
  const activeQuery = useQuery({ queryKey: ['activeWorkout'], queryFn: getActiveWorkout });

  const filtered = useMemo(() => {
    const programs = programsQuery.data ?? [];
    const q = search.trim().toLowerCase();
    return programs.filter((p) => {
      const matchesSearch =
        !q ||
        p.name.toLowerCase().includes(q) ||
        p.description.toLowerCase().includes(q) ||
        p.categoryName.toLowerCase().includes(q);
      const matchesCategory =
        !categoryId || categoriesQuery.data?.find((c) => c.id === categoryId)?.name === p.categoryName;
      return matchesSearch && matchesCategory;
    });
  }, [programsQuery.data, categoriesQuery.data, search, categoryId]);

  const featured = filtered.find((p) => p.isFeatured) ?? filtered[0];
  const popular = filtered.filter((p) => p.id !== featured?.id);

  if (programsQuery.isLoading || categoriesQuery.isLoading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator color={theme.colors.primary} size="large" />
      </View>
    );
  }

  if (programsQuery.isError) {
    return (
      <View style={styles.centered}>
        <Text style={styles.error}>{getApiErrorMessage(programsQuery.error)}</Text>
      </View>
    );
  }

  const active = activeQuery.data;
  const categories = categoriesQuery.data ?? [];
  const categoriesError = categoriesQuery.isError ? getApiErrorMessage(categoriesQuery.error) : null;

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      {categoriesError ? (
        <View style={styles.categoriesBanner}>
          <Text style={styles.categoriesBannerText}>Categories unavailable: {categoriesError}</Text>
        </View>
      ) : null}
      <View style={styles.header}>
        <Text style={styles.heading}>Programs</Text>
        <Pressable style={styles.bell} hitSlop={8}>
          <Bell color={theme.colors.text} size={22} />
          <View style={styles.bellDot} />
        </Pressable>
      </View>

      {active ? (
        <Pressable style={styles.activeBanner} onPress={() => router.push(ROUTES.activeWorkout)}>
          <Text style={styles.activeTitle}>Workout in progress</Text>
          <Text style={styles.activeSubtitle}>{active.programName}</Text>
        </Pressable>
      ) : null}

      <SearchBar value={search} onChangeText={setSearch} />

      <View style={styles.sectionHeader}>
        <Text style={styles.sectionTitle}>Categories</Text>
        <Pressable onPress={() => setCategoryId(null)}>
          <Text style={styles.link}>View All</Text>
        </Pressable>
      </View>
      <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.categories}>
        {categories.map((cat) => (
          <CategoryCard
            key={cat.id}
            category={cat}
            selected={categoryId === cat.id}
            onPress={() => setCategoryId((id) => (id === cat.id ? null : cat.id))}
          />
        ))}
      </ScrollView>

      {featured ? <FeaturedProgramCard program={featured} /> : null}

      <View style={styles.sectionHeader}>
        <Text style={styles.sectionTitle}>Popular Workouts</Text>
        <Text style={styles.link}>{popular.length} programs</Text>
      </View>

      {popular.length > 0 ? (
        popular.map((p) => <PopularWorkoutRow key={p.id} program={p} />)
      ) : (
        <Text style={styles.empty}>No programs match your search.</Text>
      )}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: theme.colors.background },
  content: { padding: theme.spacing.md, paddingBottom: theme.spacing.xl },
  centered: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: theme.colors.background },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: theme.spacing.md,
  },
  heading: { fontSize: 28, fontWeight: '700', color: theme.colors.text },
  bell: { position: 'relative', padding: 4 },
  bellDot: {
    position: 'absolute',
    top: 4,
    right: 4,
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: theme.colors.primary,
  },
  activeBanner: {
    backgroundColor: theme.colors.primary,
    padding: theme.spacing.md,
    borderRadius: theme.radius.md,
    marginBottom: theme.spacing.md,
  },
  activeTitle: { color: '#fff', fontWeight: '700', fontSize: 14 },
  activeSubtitle: { color: '#dbeafe', marginTop: 4 },
  sectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: theme.spacing.lg,
    marginBottom: theme.spacing.sm,
  },
  sectionTitle: { color: theme.colors.text, fontSize: 18, fontWeight: '700' },
  link: { color: theme.colors.primary, fontWeight: '600', fontSize: 14 },
  categories: { gap: theme.spacing.sm, paddingBottom: theme.spacing.sm },
  categoriesBanner: {
    backgroundColor: theme.colors.surface,
    borderWidth: 1,
    borderColor: theme.colors.error,
    borderRadius: theme.radius.md,
    padding: theme.spacing.sm,
    marginBottom: theme.spacing.sm,
  },
  categoriesBannerText: { color: theme.colors.error, fontSize: 13 },
  empty: { color: theme.colors.textMuted, textAlign: 'center', marginTop: theme.spacing.lg },
  error: { color: theme.colors.error },
});
