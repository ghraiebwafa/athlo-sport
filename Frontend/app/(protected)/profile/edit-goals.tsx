import { useMutation, useQueryClient } from '@tanstack/react-query';
import { router } from 'expo-router';
import { useState } from 'react';
import { Alert, KeyboardAvoidingView, Platform, ScrollView, StyleSheet, View } from 'react-native';
import { GoalSelector } from '@/components/auth/GoalSelector';
import { ScreenHeader } from '@/components/ui/ScreenHeader';
import { Button } from '@/components/ui/Button';
import { FormErrorBanner } from '@/components/ui/FormErrorBanner';
import { Input } from '@/components/ui/Input';
import { theme } from '@/constants/theme';
import { updateProfile } from '@/lib/api/auth';
import { parseApiError } from '@/lib/api/client';
import { getTokens, setTokens, useAuthStore } from '@/stores/authStore';
import type { FitnessGoal } from '@/lib/types';

export default function EditGoalsScreen() {
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();
  const [fitnessGoal, setFitnessGoal] = useState<FitnessGoal>(user?.fitnessGoal ?? 'StayActive');
  const [currentWeight, setCurrentWeight] = useState(String(user?.currentWeight ?? 70));
  const [goalWeight, setGoalWeight] = useState(String(user?.goalWeight ?? 65));
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const mutation = useMutation({
    mutationFn: () =>
      updateProfile({
        fitnessGoal,
        currentWeight: Number.parseFloat(currentWeight),
        goalWeight: Number.parseFloat(goalWeight),
      }),
    onSuccess: async (profile) => {
      const tokens = getTokens();
      if (tokens) await setTokens({ ...tokens, user: profile });
      await queryClient.invalidateQueries({ queryKey: ['profile'] });
      await queryClient.invalidateQueries({ queryKey: ['progress'] });
      Alert.alert('Saved', 'Your goals were updated.', [
        { text: 'OK', onPress: () => router.back() },
      ]);
    },
    onError: (err) => {
      const parsed = parseApiError(err);
      setError(parsed.message);
      setFieldErrors(parsed.fieldErrors);
    },
  });

  const handleSave = () => {
    setError('');
    setFieldErrors({});
    const current = Number.parseFloat(currentWeight);
    const goal = Number.parseFloat(goalWeight);
    const nextErrors: Record<string, string> = {};

    if (!Number.isFinite(current) || current < 20 || current > 500) {
      nextErrors.currentWeight = 'Enter a weight between 20 and 500 kg.';
    }
    if (!Number.isFinite(goal) || goal < 20 || goal > 500) {
      nextErrors.goalWeight = 'Enter a weight between 20 and 500 kg.';
    }
    if (Object.keys(nextErrors).length > 0) {
      setFieldErrors(nextErrors);
      setError('Please fix the highlighted fields.');
      return;
    }
    mutation.mutate();
  };

  return (
    <View style={styles.screen}>
      <ScreenHeader title="Edit Goals" onBack={() => router.back()} />
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={styles.flex}>
        <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
          <GoalSelector value={fitnessGoal} onChange={setFitnessGoal} />
          <Input
            label="Current Weight (kg)"
            value={currentWeight}
            onChangeText={setCurrentWeight}
            keyboardType="decimal-pad"
            error={fieldErrors.currentWeight}
          />
          <Input
            label="Goal Weight (kg)"
            value={goalWeight}
            onChangeText={setGoalWeight}
            keyboardType="decimal-pad"
            error={fieldErrors.goalWeight}
          />
          <FormErrorBanner message={error} />
          <Button title="Save Goals" onPress={handleSave} loading={mutation.isPending} />
        </ScrollView>
      </KeyboardAvoidingView>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.colors.background },
  flex: { flex: 1 },
  content: { padding: theme.spacing.md, gap: theme.spacing.md, paddingBottom: 40 },
});
