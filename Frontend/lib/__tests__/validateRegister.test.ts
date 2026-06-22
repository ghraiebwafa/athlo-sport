import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { validateRegisterForm } from '../validateRegister';

describe('validateRegisterForm', () => {
  it('requires email and matching passwords', () => {
    const result = validateRegisterForm({
      fullName: '',
      email: '',
      password: 'short',
      confirmPassword: 'other',
      currentWeight: '10',
      goalWeight: '10',
    });

    assert.equal(result.ok, false);
    assert.ok(result.fieldErrors.email);
    assert.ok(result.fieldErrors.password);
    assert.ok(result.fieldErrors.confirmPassword);
    assert.ok(result.fieldErrors.currentWeight);
  });

  it('accepts valid registration input', () => {
    const result = validateRegisterForm({
      fullName: 'Jane Doe',
      email: 'jane@example.com',
      password: 'Secret123',
      confirmPassword: 'Secret123',
      currentWeight: '70',
      goalWeight: '65',
    });

    assert.equal(result.ok, true);
    assert.deepEqual(result.fieldErrors, {});
  });
});
