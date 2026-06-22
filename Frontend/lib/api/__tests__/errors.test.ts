import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { AxiosError, AxiosHeaders } from 'axios';
import { parseApiError } from '../errors';

describe('parseApiError', () => {
  it('returns network message when there is no response', () => {
    const error = new AxiosError('Network Error', 'ERR_NETWORK');
    const parsed = parseApiError(error);
    assert.match(parsed.message, /Cannot reach the server/);
    assert.equal(parsed.code, 'NETWORK');
  });

  it('returns timeout message for aborted requests', () => {
    const error = new AxiosError('timeout', 'ECONNABORTED');
    const parsed = parseApiError(error);
    assert.equal(parsed.message, 'Request timed out. Please try again.');
    assert.equal(parsed.code, 'TIMEOUT');
  });

  it('maps validation details to field errors', () => {
    const error = new AxiosError(
      'Bad Request',
      'ERR_BAD_REQUEST',
      undefined,
      undefined,
      {
        status: 400,
        statusText: 'Bad Request',
        headers: {},
        config: { headers: new AxiosHeaders() },
        data: {
          api: {
            error: {
              code: 'VALIDATION_FAILED',
              message: 'One or more validation errors occurred.',
              details: [{ field: 'email', message: 'Email is required.' }],
            },
          },
        },
      }
    );

    const parsed = parseApiError(error);
    assert.equal(parsed.fieldErrors.email, 'Email is required.');
    assert.match(parsed.message, /Email is required/);
  });
});
