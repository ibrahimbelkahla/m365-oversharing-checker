using OversharingChecker.Api.Data;
using OversharingChecker.Api.Models;
using OversharingChecker.Api.Risk;

namespace OversharingChecker.Api.Services;

/// <summary>Loads items from the data source, runs the risk analysis and builds the dashboard summary.</summary>
public sealed class ScanService(ISharingDataSource dataSource, RiskAnalyzer analyzer)
{
    public const int TopSiteCount = 8;

    public async Task<IReadOnlyList<RiskAssessment>> GetAssessmentsAsync(
        ItemFilter? filter = null, CancellationToken cancellationToken = default)
    {
        var items = await dataSource.GetItemsAsync(cancellationToken);
        IEnumerable<RiskAssessment> results = analyzer.AnalyzeAll(items);

        if (filter?.Severity is { } severity) results = results.Where(r => r.HighestSeverity == severity);
        if (filter?.Location is { } location) results = results.Where(r => r.Item.LocationType == location);
        if (filter?.Label is { } label) results = results.Where(r => r.Item.SensitivityLabel == label);

        return results.ToList();
    }

    public async Task<RiskAssessment?> GetAssessmentAsync(string id, CancellationToken cancellationToken = default)
    {
        var items = await dataSource.GetItemsAsync(cancellationToken);
        var item = items.FirstOrDefault(i => i.Id == id);
        return item is null ? null : analyzer.Analyze(item);
    }

    public async Task<ScanSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var results = await GetAssessmentsAsync(cancellationToken: cancellationToken);
        var permissions = results.SelectMany(r => r.Item.Permissions).ToList();

        var topSites = results
            .GroupBy(r => (r.Item.Site, r.Item.LocationType))
            .Select(g => new SiteRisk(g.Key.Site, g.Key.LocationType, g.Count(), g.Sum(r => r.RiskScore), CountBySeverity(g)))
            .Where(s => s.TotalScore > 0)
            .OrderByDescending(s => s.TotalScore)
            .Take(TopSiteCount)
            .ToList();

        return new ScanSummary(
            TotalItems: results.Count,
            ItemsAtRisk: results.Count(r => r.Findings.Count > 0),
            ItemsBySeverity: CountBySeverity(results),
            ExternalShares: permissions.Count(p => p.IsExternalGuest),
            AnyoneLinks: permissions.Count(p => p.Type == PermissionType.AnyoneLink),
            AverageScore: results.Count == 0 ? 0 : Math.Round(results.Average(r => r.RiskScore), 1),
            TopRiskySites: topSites);
    }

    /// <summary>Counts items by their highest severity. Every severity is present, even with a count of 0.</summary>
    private static Dictionary<Severity, int> CountBySeverity(IEnumerable<RiskAssessment> results)
    {
        var counts = Enum.GetValues<Severity>().ToDictionary(s => s, _ => 0);
        foreach (var r in results)
            if (r.HighestSeverity is { } s) counts[s]++;
        return counts;
    }
}
