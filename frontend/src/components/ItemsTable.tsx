import { labelText, sharingSummary } from '../format';
import type { RiskAssessment } from '../types';
import { SeverityBadge } from './SeverityBadge';

interface Props {
  items: RiskAssessment[];
  selectedId: string | null;
  onSelect: (assessment: RiskAssessment) => void;
}

/** Items sorted by risk score (the API already returns them riskiest first). */
export function ItemsTable({ items, selectedId, onSelect }: Props) {
  if (items.length === 0) {
    return <p className="empty">No items match these filters.</p>;
  }

  return (
    <div className="table-wrap">
      <table className="items">
        <thead>
          <tr>
            <th className="col-score">Score</th>
            <th>Severity</th>
            <th>File</th>
            <th className="hide-sm">Location</th>
            <th className="hide-sm">Label</th>
            <th className="hide-md">Shared with</th>
            <th className="col-findings hide-sm">Findings</th>
          </tr>
        </thead>
        <tbody>
          {items.map((a) => (
            <tr
              key={a.item.id}
              className={a.item.id === selectedId ? 'selected' : undefined}
              onClick={() => onSelect(a)}
              onKeyDown={(e) => {
                // Rows are focusable, so keyboard users can open them too.
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault();
                  onSelect(a);
                }
              }}
              tabIndex={0}
              aria-label={`${a.item.name}, risk score ${a.riskScore}`}
            >
              <td className="col-score">
                <span className="score">{a.riskScore}</span>
                <span className="score-bar" aria-hidden="true">
                  <span
                    className={`score-fill fill-${a.highestSeverity?.toLowerCase() ?? 'none'}`}
                    style={{ width: `${a.riskScore}%` }}
                  />
                </span>
              </td>
              <td><SeverityBadge severity={a.highestSeverity} /></td>
              <td>
                <div className="file-name">{a.item.name}</div>
                <div className="file-site">{a.item.site}</div>
              </td>
              <td className="hide-sm">{a.item.locationType}</td>
              <td className="hide-sm"><span className="chip">{labelText(a.item.sensitivityLabel)}</span></td>
              <td className="hide-md muted">{sharingSummary(a.item.permissions)}</td>
              <td className="col-findings hide-sm">{a.findings.length}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
