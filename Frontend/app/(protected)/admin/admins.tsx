import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Redirect, router } from 'expo-router';
import { Plus, Trash2 } from 'lucide-react-native';
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
import { createAdmin, getAdmins, removeAdmin } from '@/lib/api/admin';
import { getApiErrorMessage, parseApiError } from '@/lib/api/client';
import { ROUTES } from '@/lib/routes';
import { isSuperAdminRole } from '@/lib/roles';
import { useAuthStore } from '@/stores/authStore';

export default function AdminAdminsScreen() {
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();
  const [showCreate, setShowCreate] = useState(false);
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');

  const query = useQuery({
    queryKey: ['admins'],
    queryFn: getAdmins,
    enabled: isSuperAdminRole(user?.role),
  });

  const createMutation = useMutation({
    mutationFn: () =>
      createAdmin({
        fullName: fullName.trim(),
        email: email.trim(),
        password,
        confirmPassword,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['admins'] });
      setShowCreate(false);
      setFullName('');
      setEmail('');
      setPassword('');
      setConfirmPassword('');
      setError('');
    },
    onError: (err) => setError(parseApiError(err).message),
  });

  const removeMutation = useMutation({
    mutationFn: removeAdmin,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['admins'] });
    },
    onError: (err) => Alert.alert('Error', getApiErrorMessage(err)),
  });

  if (!isSuperAdminRole(user?.role)) {
    return <Redirect href={ROUTES.admin} />;
  }

  return (
    <AdminGuard>
      <View style={styles.screen}>
        <ScreenHeader
          title="Admins"
          onBack={() => router.back()}
          right={
            <Pressable
              onPress={() => setShowCreate(true)}
              accessibilityRole="button"
              accessibilityLabel="Create admin"
            >
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
              !query.isLoading ? <Text style={styles.empty}>No admins found.</Text> : null
            }
            renderItem={({ item }) => (
              <View style={styles.row}>
                <View style={styles.rowBody}>
                  <Text style={styles.name}>{item.fullName}</Text>
                  <Text style={styles.meta}>
                    {item.email} · {item.role}
                  </Text>
                </View>
                {item.role !== 'SuperAdmin' ? (
                  <Pressable
                    onPress={() =>
                      Alert.alert('Remove admin?', item.email, [
                        { text: 'Cancel', style: 'cancel' },
                        {
                          text: 'Remove',
                          style: 'destructive',
                          onPress: () => removeMutation.mutate(item.id),
                        },
                      ])
                    }
                    hitSlop={8}
                    accessibilityLabel={`Remove ${item.fullName}`}
                  >
                    <Trash2 color={theme.colors.red} size={18} />
                  </Pressable>
                ) : (
                  <Text style={styles.badge}>Owner</Text>
                )}
              </View>
            )}
          />
        )}

        <Modal visible={showCreate} animationType="slide" transparent onRequestClose={() => setShowCreate(false)}>
          <KeyboardAvoidingView
            behavior={Platform.OS === 'ios' ? 'padding' : undefined}
            style={styles.modalWrap}
          >
            <View style={styles.modalCard}>
              <Text style={styles.modalTitle}>Create admin</Text>
              <Input label="Full name" value={fullName} onChangeText={setFullName} />
              <Input
                label="Email"
                value={email}
                onChangeText={setEmail}
                autoCapitalize="none"
                keyboardType="email-address"
              />
              <Input
                label="Password"
                value={password}
                onChangeText={setPassword}
                secureTextEntry
                secureToggle
              />
              <Input
                label="Confirm password"
                value={confirmPassword}
                onChangeText={setConfirmPassword}
                secureTextEntry
                secureToggle
              />
              <FormErrorBanner message={error} />
              <Button
                title="Create"
                loading={createMutation.isPending}
                onPress={() => {
                  if (!fullName.trim() || !email.trim() || !password) {
                    setError('All fields are required.');
                    return;
                  }
                  if (password !== confirmPassword) {
                    setError('Passwords do not match.');
                    return;
                  }
                  setError('');
                  createMutation.mutate();
                }}
              />
              <Button title="Cancel" variant="secondary" onPress={() => setShowCreate(false)} />
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
  badge: { color: theme.colors.primary, fontWeight: '700', fontSize: 12 },
  modalWrap: { flex: 1, justifyContent: 'flex-end', backgroundColor: 'rgba(0,0,0,0.55)' },
  modalCard: {
    backgroundColor: theme.colors.surface,
    borderTopLeftRadius: theme.radius.xl,
    borderTopRightRadius: theme.radius.xl,
    padding: theme.spacing.lg,
    gap: theme.spacing.sm,
  },
  modalTitle: { color: theme.colors.text, fontWeight: '700', fontSize: 18, marginBottom: 4 },
});
