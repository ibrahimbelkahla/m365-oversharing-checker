using OversharingChecker.Api.Explanations;
using OversharingChecker.Api.Models;
using OversharingChecker.Api.Risk;
using OversharingChecker.Api.Risk.Rules;
using static OversharingChecker.Tests.TestData;

namespace OversharingChecker.Tests.Explanations;

public class RuleBasedRiskExplainerTests
{
    private readonly RuleBasedRiskExplainer _explainer = new();

    private static RiskAssessment Assessment(params RiskFinding[] findings) =>
        new(Item(SensitivityLabel.Confidential), findings, RiskScoreCalculator.Calculate(findings),
            findings.Length > 0 ? findings.Max(f => f.Severity) : null);

    private static RiskFinding Finding(string ruleId, Severity severity = Severity.High) => new(ruleId, severity, "title", "reason");

    [Fact]
    public async Task Item_without_findings_needs_no_action()
    {
        var result = await _explainer.ExplainAsync(Assessment());

        Assert.Contains("No oversharing risks", result.Explanation);
        Assert.Empty(result.FixSteps);
        Assert.Equal(_explainer.Name, result.GeneratedBy);
    }

    [Fact]
    public async Task Business_risk_comes_from_the_most_severe_finding()
    {
        var result = await _explainer.ExplainAsync(Assessment(
            Finding(AnyoneEditLinkRule.Id, Severity.Critical),
            Finding(StaleOrNonExpiringLinkRule.Id, Severity.Medium)));

        Assert.StartsWith("Anyone who has the link can change or delete", result.Explanation);
        Assert.Contains("Strangers could alter", result.BusinessRisk);
    }

    [Fact]
    public async Task Fix_steps_are_limited_and_unique()
    {
        var result = await _explainer.ExplainAsync(Assessment(
            Finding(AnyoneEditLinkRule.Id), Finding(ExternalGuestEditRule.Id), Finding(StaleOrNonExpiringLinkRule.Id)));

        Assert.InRange(result.FixSteps.Count, 2, RuleBasedRiskExplainer.MaxFixSteps);
        Assert.Equal(result.FixSteps.Count, result.FixSteps.Distinct().Count());
    }

    [Theory]
    [InlineData(AnyoneLinkRule.Id)]
    [InlineData(ExternalGuestEditRule.Id)]
    [InlineData(ConfidentialExternalShareRule.Id)]
    [InlineData(OrganizationLinkOnConfidentialRule.Id)]
    [InlineData(StaleOrNonExpiringLinkRule.Id)]
    [InlineData(ExcessiveDirectSharingRule.Id)]
    [InlineData(AnyoneEditLinkRule.Id)]
    public async Task Every_rule_has_a_template(string ruleId)
    {
        Assert.Contains(ruleId, RuleBasedRiskExplainer.SupportedRuleIds);

        var result = await _explainer.ExplainAsync(Assessment(Finding(ruleId)));

        Assert.False(string.IsNullOrWhiteSpace(result.Explanation));
        Assert.False(string.IsNullOrWhiteSpace(result.BusinessRisk));
        Assert.NotEmpty(result.FixSteps);
    }
}
