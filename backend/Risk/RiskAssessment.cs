using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk;

/// <summary>An item together with everything the risk engine found about it.</summary>
public sealed record RiskAssessment(
    SharedItem Item,
    IReadOnlyList<RiskFinding> Findings,
    int RiskScore,
    Severity? HighestSeverity);
