import { labelText } from '../format';
import {
  LOCATION_TYPES,
  SENSITIVITY_LABELS,
  SEVERITIES,
  type ItemFilters,
  type LocationType,
  type SensitivityLabel,
  type Severity,
} from '../types';

interface Props {
  search: string;
  onSearchChange: (value: string) => void;
  filters: ItemFilters;
  onFiltersChange: (filters: ItemFilters) => void;
}

/** Search box plus the three dropdown filters, in one row above the table. */
export function Filters({ search, onSearchChange, filters, onFiltersChange }: Props) {
  return (
    <div className="filters">
      <input
        type="search"
        className="search"
        placeholder="Search files, sites, owners…"
        aria-label="Search"
        value={search}
        onChange={(e) => onSearchChange(e.target.value)}
      />
      <select
        aria-label="Severity"
        value={filters.severity ?? ''}
        onChange={(e) => onFiltersChange({ ...filters, severity: (e.target.value || undefined) as Severity | undefined })}
      >
        <option value="">All severities</option>
        {SEVERITIES.map((s) => <option key={s} value={s}>{s}</option>)}
      </select>
      <select
        aria-label="Location"
        value={filters.location ?? ''}
        onChange={(e) => onFiltersChange({ ...filters, location: (e.target.value || undefined) as LocationType | undefined })}
      >
        <option value="">All locations</option>
        {LOCATION_TYPES.map((l) => <option key={l} value={l}>{l}</option>)}
      </select>
      <select
        aria-label="Sensitivity label"
        value={filters.label ?? ''}
        onChange={(e) => onFiltersChange({ ...filters, label: (e.target.value || undefined) as SensitivityLabel | undefined })}
      >
        <option value="">All labels</option>
        {SENSITIVITY_LABELS.map((l) => <option key={l} value={l}>{labelText(l)}</option>)}
      </select>
    </div>
  );
}
