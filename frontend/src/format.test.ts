import { describe, expect, it } from 'vitest';
import { searchItems, sharingSummary } from './format';
import type { Permission, RiskAssessment } from './types';

const assessment = (name: string, site: string): RiskAssessment => ({
  item: {
    id: name, name, site, path: `/sites/${site}/${name}`, locationType: 'SharePoint',
    sensitivityLabel: 'Internal', owner: 'Lena Huber', lastModified: '2026-10-01T00:00:00Z', permissions: [],
  },
  findings: [], riskScore: 0, highestSeverity: null,
});

const permission = (overrides: Partial<Permission>): Permission => ({
  id: Math.random().toString(), type: 'SpecificPeople', role: 'Read', grantee: 'x', granteeEmail: null,
  isExternalGuest: false, linkCreated: null, expirationDate: null, isSharingLink: false, canEdit: false,
  ...overrides,
});

describe('searchItems', () => {
  const items = [assessment('Gehaltsliste.xlsx', 'HR'), assessment('Preisliste.pdf', 'Vertrieb')];

  it('returns everything for an empty query', () => {
    expect(searchItems(items, '  ')).toHaveLength(2);
  });

  it('matches name and site case-insensitively', () => {
    expect(searchItems(items, 'gehalt').map((a) => a.item.name)).toEqual(['Gehaltsliste.xlsx']);
    expect(searchItems(items, 'VERTRIEB').map((a) => a.item.name)).toEqual(['Preisliste.pdf']);
  });
});

describe('sharingSummary', () => {
  it('describes links, guests and people', () => {
    const text = sharingSummary([
      permission({ type: 'AnyoneLink', isSharingLink: true }),
      permission({ isExternalGuest: true }),
      permission({}),
      permission({}),
    ]);
    expect(text).toBe('1 anyone link · 1 external · 2 people');
  });

  it('says owner only when nothing is shared', () => {
    expect(sharingSummary([])).toBe('Owner only');
  });
});
