// Typed client for the backend API. All HTTP calls of the app go through this file.
import type {
  HealthResponse,
  ItemFilters,
  RiskAssessment,
  RiskExplanation,
  ScanSummary,
} from './types';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5110';

/** Thrown when the API answers with an error status or cannot be reached. */
export class ApiError extends Error {
  readonly status: number | undefined;

  constructor(message: string, status?: number) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${BASE_URL}${path}`, init);
  } catch {
    throw new ApiError('Cannot reach the API. Is the backend running?');
  }

  if (!response.ok) {
    // ASP.NET Core returns "problem details" JSON with a human-readable "detail" field.
    const problem = await response.json().catch(() => null);
    throw new ApiError(problem?.detail ?? `Request failed with status ${response.status}`, response.status);
  }
  return response.json() as Promise<T>;
}

/** Turns filters into a query string, skipping empty values: { severity: 'High' } -> "?severity=High". */
export function buildItemsQuery(filters: ItemFilters): string {
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(filters)) {
    if (value) params.set(key, value);
  }
  const query = params.toString();
  return query ? `?${query}` : '';
}

export const api = {
  getHealth: () => request<HealthResponse>('/api/health'),
  getSummary: () => request<ScanSummary>('/api/summary'),
  getItems: (filters: ItemFilters = {}) => request<RiskAssessment[]>(`/api/items${buildItemsQuery(filters)}`),
  getItem: (id: string) => request<RiskAssessment>(`/api/items/${encodeURIComponent(id)}`),
  explainItem: (id: string) =>
    request<RiskExplanation>(`/api/items/${encodeURIComponent(id)}/explain`, { method: 'POST' }),
};
