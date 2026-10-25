import { Bar, BarChart, CartesianGrid, Legend, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { SEVERITY_COLORS } from '../format';
import { SEVERITIES, type Severity, type SiteRisk } from '../types';

interface Props {
  sites: SiteRisk[];
  theme: 'light' | 'dark';
}

/** Stacked horizontal bars: how many risky items each site/team has, split by severity. */
export function SiteRiskChart({ sites, theme }: Props) {
  const colors = SEVERITY_COLORS[theme];
  const surface = theme === 'dark' ? '#1d1f23' : '#ffffff';
  const muted = theme === 'dark' ? '#a3a8b3' : '#5d6470';
  const grid = theme === 'dark' ? '#2d3038' : '#e8eaee';

  // Recharts wants one flat object per bar.
  const data = sites.map((s) => ({ site: s.site, ...s.itemsBySeverity }));

  return (
    <div className="chart" style={{ height: 48 + sites.length * 34 }}>
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={data} layout="vertical" margin={{ top: 0, right: 16, bottom: 0, left: 0 }} barCategoryGap={8}>
          <CartesianGrid horizontal={false} stroke={grid} />
          <XAxis type="number" allowDecimals={false} tick={{ fill: muted, fontSize: 12 }} axisLine={false} tickLine={false} />
          <YAxis type="category" dataKey="site" width={130} tick={{ fill: muted, fontSize: 12 }} axisLine={false} tickLine={false} />
          <Tooltip
            cursor={{ fill: grid, opacity: 0.5 }}
            contentStyle={{ background: surface, border: `1px solid ${grid}`, borderRadius: 8, fontSize: 13 }}
            labelStyle={{ fontWeight: 600, color: theme === 'dark' ? '#eceef2' : '#16181d' }}
            itemStyle={{ color: muted }}
            formatter={(value, name) => [`${value} item${value === 1 ? '' : 's'}`, name]}
          />
          <Legend verticalAlign="top" align="left" height={32} iconType="circle" iconSize={9}
            // Keep the severity order (Critical -> Low) instead of sorting alphabetically.
            itemSorter={(entry) => SEVERITIES.indexOf(entry.value as Severity)}
            formatter={(value) => <span style={{ color: muted, fontSize: 13 }}>{value}</span>} />
          {/* The surface-colored stroke leaves a small gap between stacked segments. */}
          {SEVERITIES.map((severity) => (
            <Bar key={severity} dataKey={severity} stackId="severity" fill={colors[severity]}
              stroke={surface} strokeWidth={2} maxBarSize={22} />
          ))}
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
