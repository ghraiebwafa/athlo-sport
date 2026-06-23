export interface PasswordValidationResult {
  ok: boolean;
  message: string;
}

/** Shared password rules aligned with backend validators. */
export function validatePassword(password: string, minLength = 8): PasswordValidationResult {
  if (!password) return { ok: false, message: 'Password is required.' };
  if (password.length < minLength) {
    return { ok: false, message: `Password must be at least ${minLength} characters.` };
  }
  if (!/[A-Z]/.test(password)) {
    return { ok: false, message: 'Password must contain an uppercase letter.' };
  }
  if (!/[0-9]/.test(password)) {
    return { ok: false, message: 'Password must contain a number.' };
  }
  return { ok: true, message: '' };
}

export function validateEmail(email: string): PasswordValidationResult {
  const trimmed = email.trim();
  if (!trimmed) return { ok: false, message: 'Email is required.' };
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmed)) {
    return { ok: false, message: 'Enter a valid email address.' };
  }
  return { ok: true, message: '' };
}
