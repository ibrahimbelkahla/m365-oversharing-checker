using OversharingChecker.Api.Models;
using OversharingChecker.Api.Risk;

namespace OversharingChecker.Tests.Risk;

public class RiskScoreCalculatorTests
{
    private static RiskFinding F(Severity severity) => new("rule", severity, "title", "reason");

    [Fact]
    public void No_findings_means_zero() => Assert.Equal(0, RiskScoreCalculator.Calculate([]));

    [Theory]
    [InlineData(Severity.Low, 15)]
    [InlineData(Severity.Medium, 35)]
    [InlineData(Severity.High, 60)]
    [InlineData(Severity.Critical, 80)]
    public void Single_finding_uses_base_score(Severity severity, int expected) =>
        Assert.Equal(expected, RiskScoreCalculator.Calculate([F(severity)]));

    [Fact]
    public void Highest_severity_sets_base_and_extra_findings_add_points() =>
        Assert.Equal(60 + 2 * 5, RiskScoreCalculator.Calculate([F(Severity.Low), F(Severity.High), F(Severity.Medium)]));

    [Fact]
    public void Score_is_capped_at_100() =>
        Assert.Equal(100, RiskScoreCalculator.Calculate(Enumerable.Repeat(F(Severity.Critical), 10).ToList()));
}
