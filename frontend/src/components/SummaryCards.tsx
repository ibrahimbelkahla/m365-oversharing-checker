import { SEVERITIES, type ScanSummary } from '../types';

export function SummaryCards({ summary }: { summary: ScanSummary }) {
  return (
    <section className="stats" aria-label="Scan summary">
      <div className="stat">
        <span className="stat-label">Items scanned</span>
        <span className="stat-value">{summary.totalItems}</span>
        <span className="stat-note">{summary.itemsAtRisk} with risks</span>
      </div>

      {SEVERITIES.map((severity) => (
        <div className="stat" key={severity}>
          <span className="stat-label">
            <span className={`swatch swatch-${severity.toLowerCase()}`} aria-hidden="true" />
            {severity}
          </span>
          <span className="stat-value">{summary.itemsBySeverity[severity]}</span>
          <span className="stat-note">items</span>
        </div>
      ))}

      <div className="stat">
        <span className="stat-label">External shares</span>
        <span className="stat-value">{summary.externalShares}</span>
        <span className="stat-note">{summary.anyoneLinks} anyone links</span>
      </div>

      <div className="stat">
        <span className="stat-label">Average score</span>
        <span className="stat-value">{summary.averageScore}</span>
        <span className="stat-note">out of 100</span>
      </div>
    </section>
  );
}
