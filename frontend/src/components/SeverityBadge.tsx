import type { Severity } from '../types';

/** Colored badge with a text label, so severity is never shown by color alone. */
export function SeverityBadge({ severity }: { severity: Severity | null }) {
  if (!severity) return <span className="badge badge-none">No risk</span>;

  return (
    <span className={`badge badge-${severity.toLowerCase()}`}>
      <span className="badge-dot" aria-hidden="true" />
      {severity}
    </span>
  );
}
