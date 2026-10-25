import { afterEach, describe, expect, it, vi } from 'vitest';
import { api, ApiError, buildItemsQuery } from './api';

afterEach(() => vi.unstubAllGlobals());

describe('buildItemsQuery', () => {
  it('is empty without filters', () => {
    expect(buildItemsQuery({})).toBe('');
  });

  it('only includes filters that are set', () => {
    expect(buildItemsQuery({ severity: 'High', location: undefined, label: 'Confidential' }))
      .toBe('?severity=High&label=Confidential');
  });
});

describe('api', () => {
  it('uses the problem details message from the backend', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ detail: 'The AI service could not create an explanation.' }), { status: 502 }),
    ));

    await expect(api.explainItem('aw-001')).rejects.toEqual(
      new ApiError('The AI service could not create an explanation.', 502),
    );
  });

  it('reports a friendly message when the backend is not running', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')));

    await expect(api.getSummary()).rejects.toThrow('Cannot reach the API');
  });
});
