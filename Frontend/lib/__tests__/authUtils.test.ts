import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { parseStoredSession } from '@/lib/parseStoredSession';
import { normalizeRouteParam } from '@/lib/routeParams';
import { validateEmail, validatePassword } from '@/lib/validatePassword';

describe('validatePassword', () => {
  it('rejects short passwords', () => {
    const result = validatePassword('Ab1');
    assert.equal(result.ok, false);
  });

  it('accepts valid passwords', () => {
    const result = validatePassword('Password1');
    assert.equal(result.ok, true);
  });
});

describe('validateEmail', () => {
  it('rejects invalid email', () => {
    assert.equal(validateEmail('not-an-email').ok, false);
  });

  it('accepts valid email', () => {
    assert.equal(validateEmail('user@example.com').ok, true);
  });
});

describe('parseStoredSession', () => {
  it('returns null for invalid JSON', () => {
    assert.equal(parseStoredSession('{bad json'), null);
  });

  it('parses a valid session', () => {
    const session = parseStoredSession(
      JSON.stringify({
        accessToken: 'access',
        refreshToken: 'refresh',
        expiresAt: new Date(Date.now() + 60_000).toISOString(),
        user: {
          id: '11111111-1111-1111-1111-111111111111',
          email: 'user@test.local',
          fullName: 'User',
          role: 'User',
          currentWeight: 70,
          goalWeight: 65,
          fitnessGoal: 'LoseWeight',
          goalProgressPercent: 0,
        },
      })
    );
    assert.ok(session);
    assert.equal(session?.accessToken, 'access');
  });
});

describe('normalizeRouteParam', () => {
  it('returns first value from array params', () => {
    assert.equal(normalizeRouteParam(['a', 'b']), 'a');
  });
});
