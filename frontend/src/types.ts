// TypeScript types matching the C# models returned by the backend API.

export type Severity = 'Low' | 'Medium' | 'High' | 'Critical';
export type LocationType = 'SharePoint' | 'OneDrive' | 'Teams';
export type SensitivityLabel = 'Public' | 'Internal' | 'Confidential' | 'HighlyConfidential';
export type PermissionType = 'AnyoneLink' | 'OrganizationLink' | 'SpecificPeople';
export type PermissionRole = 'Read' | 'Write' | 'Owner';

export const SEVERITIES: Severity[] = ['Critical', 'High', 'Medium', 'Low'];
export const LOCATION_TYPES: LocationType[] = ['SharePoint', 'Teams', 'OneDrive'];
export const SENSITIVITY_LABELS: SensitivityLabel[] = ['HighlyConfidential', 'Confidential', 'Internal', 'Public'];

export interface Permission {
  id: string;
  type: PermissionType;
  role: PermissionRole;
  grantee: string;
  granteeEmail: string | null;
  isExternalGuest: boolean;
  linkCreated: string | null;
  expirationDate: string | null;
  isSharingLink: boolean;
  canEdit: boolean;
}

export interface SharedItem {
  id: string;
  name: string;
  path: string;
  site: string;
  locationType: LocationType;
  sensitivityLabel: SensitivityLabel;
  owner: string;
  lastModified: string;
  permissions: Permission[];
}

export interface RiskFinding {
  ruleId: string;
  severity: Severity;
  title: string;
  reason: string;
}

export interface RiskAssessment {
  item: SharedItem;
  findings: RiskFinding[];
  riskScore: number;
  highestSeverity: Severity | null;
}

export interface SiteRisk {
  site: string;
  locationType: LocationType;
  itemCount: number;
  totalScore: number;
  itemsBySeverity: Record<Severity, number>;
}

export interface ScanSummary {
  totalItems: number;
  itemsAtRisk: number;
  itemsBySeverity: Record<Severity, number>;
  externalShares: number;
  anyoneLinks: number;
  averageScore: number;
  topRiskySites: SiteRisk[];
}

export interface RiskExplanation {
  explanation: string;
  businessRisk: string;
  fixSteps: string[];
  generatedBy: string;
}

export interface HealthResponse {
  status: string;
  explainer: string;
  aiEnabled: boolean;
}

export interface ItemFilters {
  severity?: Severity;
  location?: LocationType;
  label?: SensitivityLabel;
}
