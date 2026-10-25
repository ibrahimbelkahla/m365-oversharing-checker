import { useEffect, useState } from 'react';
import { formatDate, labelText } from '../format';
import type { Permission, RiskAssessment } from '../types';
import { ExplainPanel } from './ExplainPanel';
import { SeverityBadge } from './SeverityBadge';

interface Props {
  assessment: RiskAssessment;
  aiEnabled: boolean;
  onClose: () => void;
}

const COLLAPSED_PEOPLE = 8;

/** Side panel with an item's findings, the AI explanation and all permissions. */
export function ItemDetail({ assessment, aiEnabled, onClose }: Props) {
  const { item, findings, riskScore, highestSeverity } = assessment;

  // Close with the Escape key.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => e.key === 'Escape' && onClose();
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [onClose]);

  const links = item.permissions.filter((p) => p.isSharingLink);
  const external = item.permissions.filter((p) => p.isExternalGuest);
  const internal = item.permissions.filter((p) => p.type === 'SpecificPeople' && !p.isExternalGuest);

  return (
    <>
      <div className="backdrop" onClick={onClose} />
      <aside className="drawer" role="dialog" aria-modal="true" aria-labelledby="detail-title">
        <header className="drawer-head">
          <div>
            <h2 id="detail-title">{item.name}</h2>
            <p className="muted small path">{item.path}</p>
          </div>
          <button className="btn-icon" onClick={onClose} aria-label="Close details">✕</button>
        </header>

        <div className="drawer-body">
          <dl className="facts">
            <div><dt>Risk score</dt><dd><strong>{riskScore}</strong> / 100 <SeverityBadge severity={highestSeverity} /></dd></div>
            <div><dt>Label</dt><dd>{labelText(item.sensitivityLabel)}</dd></div>
            <div><dt>Location</dt><dd>{item.locationType} · {item.site}</dd></div>
            <div><dt>Owner</dt><dd>{item.owner}</dd></div>
            <div><dt>Last modified</dt><dd>{formatDate(item.lastModified)}</dd></div>
          </dl>

          <section>
            <h3>Findings ({findings.length})</h3>
            {findings.length === 0 ? (
              <p className="muted">No oversharing risks found.</p>
            ) : (
              <ul className="findings">
                {findings.map((f) => (
                  <li key={f.ruleId}>
                    <SeverityBadge severity={f.severity} />
                    <div>
                      <div className="finding-title">{f.title}</div>
                      <div className="muted small">{f.reason}</div>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <ExplainPanel key={item.id} itemId={item.id} aiEnabled={aiEnabled} />

          <section>
            <h3>Permissions ({item.permissions.length})</h3>
            {item.permissions.length === 0 && <p className="muted">Only the owner has access.</p>}
            <PermissionGroup title="Sharing links" permissions={links} />
            <PermissionGroup title="External guests" permissions={external} />
            <PermissionGroup title="People in the organization" permissions={internal} collapsible />
          </section>
        </div>
      </aside>
    </>
  );
}

function PermissionGroup({ title, permissions, collapsible = false }: {
  title: string;
  permissions: Permission[];
  collapsible?: boolean;
}) {
  const [expanded, setExpanded] = useState(false);
  if (permissions.length === 0) return null;

  const visible = collapsible && !expanded ? permissions.slice(0, COLLAPSED_PEOPLE) : permissions;

  return (
    <div className="perm-group">
      <h4>{title}</h4>
      <ul className="perms">
        {visible.map((p) => (
          <li key={p.id}>
            <div>
              <div>{p.grantee}</div>
              {p.granteeEmail && <div className="muted small">{p.granteeEmail}</div>}
              {p.isSharingLink && (
                <div className="muted small">
                  Created {formatDate(p.linkCreated)} · {p.expirationDate ? `expires ${formatDate(p.expirationDate)}` : 'never expires'}
                </div>
              )}
            </div>
            <span className={`role${p.canEdit ? ' role-edit' : ''}`}>{p.canEdit ? 'Can edit' : 'Can view'}</span>
          </li>
        ))}
      </ul>
      {collapsible && permissions.length > COLLAPSED_PEOPLE && (
        <button className="btn-link" onClick={() => setExpanded(!expanded)}>
          {expanded ? 'Show fewer' : `Show all ${permissions.length}`}
        </button>
      )}
    </div>
  );
}
