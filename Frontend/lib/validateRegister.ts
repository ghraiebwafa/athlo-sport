import { validateEmail, validatePassword } from '@/lib/validatePassword';

export interface RegisterFormValues {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  currentWeight: string;
  goalWeight: string;
}

export interface RegisterValidationResult {
  ok: boolean;
  message: string;
  fieldErrors: Record<string, string>;
}

export function validateRegisterForm(values: RegisterFormValues): RegisterValidationResult {
  const fieldErrors: Record<string, string> = {};
  const fullName = values.fullName.trim();

  if (!fullName) fieldErrors.fullName = 'Full name is required.';
  else if (fullName.length > 100) fieldErrors.fullName = 'Full name must be 100 characters or fewer.';

  const emailResult = validateEmail(values.email);
  if (!emailResult.ok) fieldErrors.email = emailResult.message;

  const passwordResult = validatePassword(values.password);
  if (!passwordResult.ok) fieldErrors.password = passwordResult.message;

  if (!values.confirmPassword) fieldErrors.confirmPassword = 'Please confirm your password.';
  else if (values.confirmPassword !== values.password) fieldErrors.confirmPassword = 'Passwords do not match.';

  const weight = Number.parseFloat(values.currentWeight);
  if (!Number.isFinite(weight) || weight < 20 || weight > 500) {
    fieldErrors.currentWeight = 'Must be between 20 and 500 kg.';
  }

  const target = Number.parseFloat(values.goalWeight);
  if (!Number.isFinite(target) || target < 20 || target > 500) {
    fieldErrors.goalWeight = 'Must be between 20 and 500 kg.';
  }

  const firstError = Object.values(fieldErrors)[0];
  return {
    ok: Object.keys(fieldErrors).length === 0,
    message: firstError ?? '',
    fieldErrors,
  };
}
