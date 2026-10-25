// Small display helpers shared by several components.
import type { Permission, RiskAssessment, SensitivityLabel, Severity } from './types';

export const labelText = (label: SensitivityLabel): string =>
  label === 'HighlyConfidential' ? 'Highly Confidential' : label;

const dateFormat = new Intl.DateTimeFormat('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
export const formatDate = (iso: string | null): string => (iso ? dateFormat.format(new Date(iso)) : '—');

/** Severity colors per theme (validated for color-blind separation; always shown with a text label). */
export const SEVERITY_COLORS: Record<'light' | 'dark', Record<Severity, string>> = {
  light: { Critical: '#a8201a', High: '#ec6a2c', Medium: '#f2b81b', Low: '#3987e5' },
  dark: { Critical: '#c92a5a', High: '#ec6a2c', Medium: '#f2b81b', Low: '#3987e5' },
};

/** Case-insensitive search over name, site, path and owner. */
export function searchItems(items: RiskAssessment[], query: string): RiskAssessment[] {
  const q = query.trim().toLowerCase();
  if (!q) return items;
  return items.filter(({ item }) =>
    [item.name, item.site, item.path, item.owner].some((field) => field.toLowerCase().includes(q)),
  );
}

/** Short sharing overview for the table, e.g. "1 anyone link · 2 external". */
export function sharingSummary(permissions: Permission[]): string {
  const anyone = permissions.filter((p) => p.type === 'AnyoneLink').length;
  const org = permissions.filter((p) => p.type === 'OrganizationLink').length;
  const external = permissions.filter((p) => p.isExternalGuest).length;
  const people = permissions.filter((p) => p.type === 'SpecificPeople' && !p.isExternalGuest).length;

  const parts = [
    anyone && `${anyone} anyone link${anyone > 1 ? 's' : ''}`,
    org && `${org} org link${org > 1 ? 's' : ''}`,
    external && `${external} external`,
    people && `${people} ${people > 1 ? 'people' : 'person'}`,
  ].filter(Boolean);
  return parts.length ? parts.join(' · ') : 'Owner only';
}
