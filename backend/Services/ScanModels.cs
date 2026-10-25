using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Services;

/// <summary>Optional filters for GET /api/items. Null means "no filter".</summary>
/// <param name="Severity">Matches items whose highest finding has this severity.</param>
public sealed record ItemFilter(Severity? Severity = null, LocationType? Location = null, SensitivityLabel? Label = null);

/// <summary>Dashboard numbers returned by GET /api/summary.</summary>
public sealed record ScanSummary(
    int TotalItems,
    int ItemsAtRisk,
    IReadOnlyDictionary<Severity, int> ItemsBySeverity,
    int ExternalShares,
    int AnyoneLinks,
    double AverageScore,
    IReadOnlyList<SiteRisk> TopRiskySites);

/// <summary>Risk totals for one SharePoint site, Team or OneDrive.</summary>
public sealed record SiteRisk(
    string Site,
    LocationType LocationType,
    int ItemCount,
    int TotalScore,
    IReadOnlyDictionary<Severity, int> ItemsBySeverity);
