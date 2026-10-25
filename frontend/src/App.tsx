import { useCallback, useEffect, useMemo, useState } from 'react';
import { api } from './api';
import { Filters } from './components/Filters';
import { ItemDetail } from './components/ItemDetail';
import { ItemsTable } from './components/ItemsTable';
import { SiteRiskChart } from './components/SiteRiskChart';
import { SummaryCards } from './components/SummaryCards';
import { searchItems } from './format';
import { useTheme } from './useTheme';
import type { HealthResponse, ItemFilters, RiskAssessment, ScanSummary } from './types';

export default function App() {
  const { theme, toggleTheme } = useTheme();

  const [health, setHealth] = useState<HealthResponse | null>(null);
  const [summary, setSummary] = useState<ScanSummary | null>(null);
  const [items, setItems] = useState<RiskAssessment[]>([]);
  const [filters, setFilters] = useState<ItemFilters>({});
  const [search, setSearch] = useState('');
  const [selected, setSelected] = useState<RiskAssessment | null>(null);
  const [loadingItems, setLoadingItems] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Health and summary are loaded once.
  useEffect(() => {
    Promise.all([api.getHealth(), api.getSummary()])
      .then(([h, s]) => { setHealth(h); setSummary(s); })
      .catch((e: Error) => setError(e.message));
  }, []);

  // Items are reloaded whenever a filter changes (filtering happens on the server).
  useEffect(() => {
    let cancelled = false;
    setLoadingItems(true);
    api.getItems(filters)
      .then((result) => !cancelled && setItems(result))
      .catch((e: Error) => !cancelled && setError(e.message))
      .finally(() => !cancelled && setLoadingItems(false));
    return () => { cancelled = true; };
  }, [filters]);

  // Free-text search runs in the browser, so typing feels instant.
  const visibleItems = useMemo(() => searchItems(items, search), [items, search]);
  const closeDetail = useCallback(() => setSelected(null), []);

  return (
    <div className="app">
      <header className="topbar">
        <div className="brand">
          <svg className="logo" viewBox="0 0 24 24" aria-hidden="true">
            <path d="M12 2 4 5v6c0 5 3.4 9.4 8 11 4.6-1.6 8-6 8-11V5l-8-3Z" fill="currentColor" opacity=".18" />
            <path d="M12 2 4 5v6c0 5 3.4 9.4 8 11 4.6-1.6 8-6 8-11V5l-8-3Z" fill="none" stroke="currentColor" strokeWidth="1.6" />
            <path d="m8.5 12 2.5 2.5 4.5-5" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
          </svg>
          <div>
            <h1>M365 Oversharing Checker</h1>
            <p className="muted small">Alpenwerk GmbH · sample tenant data</p>
          </div>
        </div>
        <div className="topbar-actions">
          {health && (
            <span className={`pill${health.aiEnabled ? ' pill-ai' : ''}`} title="Active explanation engine">
              {health.aiEnabled ? `AI: ${health.explainer}` : 'Explanations: rule-based'}
            </span>
          )}
          <button className="btn-icon" onClick={toggleTheme} aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}>
            {theme === 'dark' ? '☀' : '☾'}
          </button>
        </div>
      </header>

      <main className="content">
        {error && <div className="alert" role="alert">{error}</div>}

        {summary && <SummaryCards summary={summary} />}

        {summary && (
          <section className="card">
            <h2>Riskiest sites and teams</h2>
            <p className="muted small">Items with findings, by highest severity</p>
            <SiteRiskChart sites={summary.topRiskySites} theme={theme} />
          </section>
        )}

        <section className="card">
          <div className="card-head">
            <div>
              <h2>Shared items</h2>
              <p className="muted small">
                {loadingItems ? 'Loading…' : `${visibleItems.length} item${visibleItems.length === 1 ? '' : 's'}, riskiest first`}
              </p>
            </div>
            <Filters search={search} onSearchChange={setSearch} filters={filters} onFiltersChange={setFilters} />
          </div>
          <ItemsTable items={visibleItems} selectedId={selected?.item.id ?? null} onSelect={setSelected} />
        </section>
      </main>

      {selected && <ItemDetail assessment={selected} aiEnabled={health?.aiEnabled ?? false} onClose={closeDetail} />}
    </div>
  );
}
