using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk;

/// <summary>Runs every registered rule against items and scores the results.</summary>
public sealed class RiskAnalyzer(IEnumerable<IRiskRule> rules)
{
    private readonly IReadOnlyList<IRiskRule> _rules = rules.ToList();

    public RiskAssessment Analyze(SharedItem item)
    {
        var findings = _rules
            .Select(rule => rule.Evaluate(item))
            .OfType<RiskFinding>() // drops the nulls (rules that found nothing)
            .OrderByDescending(f => f.Severity)
            .ToList();

        return new RiskAssessment(
            item,
            findings,
            RiskScoreCalculator.Calculate(findings),
            findings.Count > 0 ? findings[0].Severity : null);
    }

    /// <summary>Analyzes all items, riskiest first.</summary>
    public IReadOnlyList<RiskAssessment> AnalyzeAll(IEnumerable<SharedItem> items) =>
        items.Select(Analyze)
            .OrderByDescending(a => a.RiskScore)
            .ThenBy(a => a.Item.Name)
            .ToList();
}
