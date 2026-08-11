import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { router } from 'expo-router';
import { Pencil, Plus, Trash2 } from 'lucide-react-native';
import { useState } from 'react';
import {
  Alert,
  FlatList,
  KeyboardAvoidingView,
  Modal,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { AdminGuard } from '@/components/auth/AdminGuard';
import { Button } from '@/components/ui/Button';
import { FormErrorBanner } from '@/components/ui/FormErrorBanner';
import { Input } from '@/components/ui/Input';
import { QueryState } from '@/components/ui/QueryState';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { theme } from '@/constants/theme';
import { createExercise, deleteExercise, getExercises, updateExercise } from '@/lib/api/admin';
import { getApiErrorMessage, parseApiError } from '@/lib/api/client';
import type { Exercise } from '@/lib/types';

export default function AdminExercisesScreen() {
  const queryClient = useQueryClient();
  const [editor, setEditor] = useState<null | { mode: 'create' | 'edit'; item?: Exercise }>(null);
  const [name, setName] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [error, setError] = useState('');

  const query = useQuery({ queryKey: ['exercises'], queryFn: getExercises });

  const saveMutation = useMutation({
    mutationFn: async () => {
      const payload = {
        name: name.trim(),
        imageUrl: imageUrl.trim() || undefined,
      };
      if (editor?.mode === 'edit' && editor.item) {
        return updateExercise(editor.item.id, payload);
      }
      return createExercise(payload);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['exercises'] });
      closeEditor();
    },
    onError: (err) => setError(parseApiError(err).message),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteExercise,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['exercises'] });
    },
    onError: (err) => Alert.alert('Error', getApiErrorMessage(err)),
  });

  const openCreate = () => {
    setName('');
    setImageUrl('');
    setError('');
    setEditor({ mode: 'create' });
  };

  const openEdit = (item: Exercise) => {
    setName(item.name);
    setImageUrl(item.imageUrl ?? '');
    setError('');
    setEditor({ mode: 'edit', item });
  };

  const closeEditor = () => {
    setEditor(null);
    setError('');
  };

  const confirmDelete = (item: Exercise) => {
    Alert.alert('Delete exercise?', item.name, [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Delete',
        style: 'destructive',
        onPress: () => deleteMutation.mutate(item.id),
      },
    ]);
  };

  return (
    <AdminGuard>
      <View style={styles.screen}>
        <ScreenHeader
          title="Exercises"
          onBack={() => router.back()}
          right={
            <Pressable onPress={openCreate} accessibilityRole="button" accessibilityLabel="Add exercise">
              <Plus color={theme.colors.primary} size={24} />
            </Pressable>
          }
        />

        {query.isError ? (
          <QueryState message={getApiErrorMessage(query.error)} onRetry={() => void query.refetch()} />
        ) : (
          <FlatList
            data={query.data ?? []}
            keyExtractor={(item) => item.id}
            contentContainerStyle={styles.list}
            refreshing={query.isFetching}
            onRefresh={() => void query.refetch()}
            ListEmptyComponent={
              !query.isLoading ? <Text style={styles.empty}>No exercises yet.</Text> : null
            }
            renderItem={({ item }) => (
              <View style={styles.row}>
                <View style={styles.rowBody}>
                  <Text style={styles.name}>{item.name}</Text>
                  {item.imageUrl ? (
                    <Text style={styles.meta} numberOfLines={1}>
                      {item.imageUrl}
                    </Text>
                  ) : null}
                </View>
                <Pressable onPress={() => openEdit(item)} hitSlop={8} accessibilityLabel={`Edit ${item.name}`}>
                  <Pencil color={theme.colors.primary} size={18} />
                </Pressable>
                <Pressable
                  onPress={() => confirmDelete(item)}
                  hitSlop={8}
                  accessibilityLabel={`Delete ${item.name}`}
                >
                  <Trash2 color={theme.colors.red} size={18} />
                </Pressable>
              </View>
            )}
          />
        )}

        <Modal visible={!!editor} animationType="slide" transparent onRequestClose={closeEditor}>
          <KeyboardAvoidingView
            behavior={Platform.OS === 'ios' ? 'padding' : undefined}
            style={styles.modalWrap}
          >
            <View style={styles.modalCard}>
              <Text style={styles.modalTitle}>
                {editor?.mode === 'edit' ? 'Edit exercise' : 'New exercise'}
              </Text>
              <Input label="Name" value={name} onChangeText={setName} autoFocus />
              <Input
                label="Image URL (optional)"
                value={imageUrl}
                onChangeText={setImageUrl}
                autoCapitalize="none"
              />
              <FormErrorBanner message={error} />
              <Button
                title="Save"
                onPress={() => {
                  if (!name.trim()) {
                    setError('Name is required.');
                    return;
                  }
                  setError('');
                  saveMutation.mutate();
                }}
                loading={saveMutation.isPending}
              />
              <Button title="Cancel" variant="secondary" onPress={closeEditor} />
            </View>
          </KeyboardAvoidingView>
        </Modal>
      </View>
    </AdminGuard>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  list: { padding: theme.spacing.md, paddingBottom: 40, gap: 8 },
  empty: { color: theme.colors.textMuted, textAlign: 'center', marginTop: 40 },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: theme.spacing.md,
  },
  rowBody: { flex: 1 },
  name: { color: theme.colors.text, fontWeight: '600', fontSize: 16 },
  meta: { color: theme.colors.textMuted, fontSize: 12, marginTop: 2 },
  modalWrap: {
    flex: 1,
    justifyContent: 'flex-end',
    backgroundColor: 'rgba(0,0,0,0.55)',
  },
  modalCard: {
    backgroundColor: theme.colors.surface,
    borderTopLeftRadius: theme.radius.xl,
    borderTopRightRadius: theme.radius.xl,
    padding: theme.spacing.lg,
    gap: theme.spacing.sm,
  },
  modalTitle: { color: theme.colors.text, fontWeight: '700', fontSize: 18, marginBottom: 4 },
});
