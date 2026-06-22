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
  const email = values.email.trim();

  if (!fullName) fieldErrors.fullName = 'Full name is required.';
  else if (fullName.length > 100) fieldErrors.fullName = 'Full name must be 100 characters or fewer.';

  if (!email) fieldErrors.email = 'Email is required.';
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) fieldErrors.email = 'Enter a valid email address.';

  if (!values.password) fieldErrors.password = 'Password is required.';
  else {
    if (values.password.length < 8) fieldErrors.password = 'Password must be at least 8 characters.';
    else if (!/[A-Z]/.test(values.password)) fieldErrors.password = 'Password must contain an uppercase letter.';
    else if (!/[0-9]/.test(values.password)) fieldErrors.password = 'Password must contain a number.';
  }

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
