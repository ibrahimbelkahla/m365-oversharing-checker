using OversharingChecker.Api.Data;
using OversharingChecker.Api.Models;
using OversharingChecker.Api.Risk;
using OversharingChecker.Api.Risk.Rules;
using static OversharingChecker.Tests.TestData;

namespace OversharingChecker.Tests.Risk;

public class RiskAnalyzerTests
{
    private static RiskAnalyzer CreateAnalyzer() => new(
    [
        new AnyoneLinkRule(),
        new ExternalGuestEditRule(),
        new ConfidentialExternalShareRule(),
        new OrganizationLinkOnConfidentialRule(),
        new StaleOrNonExpiringLinkRule(new FixedTimeProvider(Now)),
        new ExcessiveDirectSharingRule(),
        new AnyoneEditLinkRule()
    ]);

    [Fact]
    public void Safe_item_has_no_findings_and_zero_score()
    {
        var result = CreateAnalyzer().Analyze(Item(SensitivityLabel.Confidential, Internal(), Internal(PermissionRole.Write)));

        Assert.Empty(result.Findings);
        Assert.Equal(0, result.RiskScore);
        Assert.Null(result.HighestSeverity);
    }

    [Fact]
    public void Findings_from_several_rules_are_combined_most_severe_first()
    {
        // Anyone edit link without expiration + external guest who can edit, on a Highly Confidential file.
        var item = Item(SensitivityLabel.HighlyConfidential,
            AnyoneLink(PermissionRole.Write, expiresInDays: null),
            External(PermissionRole.Write));

        var result = CreateAnalyzer().Analyze(item);

        Assert.Equal(
            [AnyoneEditLinkRule.Id, ConfidentialExternalShareRule.Id, ExternalGuestEditRule.Id, StaleOrNonExpiringLinkRule.Id],
            result.Findings.Select(f => f.RuleId).Order());
        Assert.Equal(
            [Severity.Critical, Severity.Critical, Severity.High, Severity.Medium],
            result.Findings.Select(f => f.Severity));
        Assert.Equal(Severity.Critical, result.HighestSeverity);
        Assert.Equal(80 + 3 * 5, result.RiskScore);
    }

    [Fact]
    public void AnalyzeAll_sorts_riskiest_first()
    {
        var safe = Item(SensitivityLabel.Internal, Internal()) with { Id = "safe" };
        var risky = Item(SensitivityLabel.Internal, AnyoneLink(PermissionRole.Write)) with { Id = "risky" };

        var results = CreateAnalyzer().AnalyzeAll([safe, risky]);

        Assert.Equal(["risky", "safe"], results.Select(r => r.Item.Id));
    }

    [Fact]
    public async Task Sample_data_has_a_realistic_mix_of_safe_and_risky_items()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "sample-data.json");
        var items = await new JsonSharingDataSource(path).GetItemsAsync();

        var results = CreateAnalyzer().AnalyzeAll(items);

        Assert.Contains(results, r => r.Findings.Count == 0);
        foreach (var severity in Enum.GetValues<Severity>())
            Assert.Contains(results, r => r.HighestSeverity == severity);
    }
}
