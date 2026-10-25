using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk;

/// <summary>
/// Turns findings into a score from 0 to 100.
/// The most severe finding sets the base score; every additional finding adds a few points.
/// </summary>
public static class RiskScoreCalculator
{
    public const int PointsPerExtraFinding = 5;

    public static int BaseScore(Severity severity) => severity switch
    {
        Severity.Critical => 80,
        Severity.High => 60,
        Severity.Medium => 35,
        Severity.Low => 15,
        _ => throw new ArgumentOutOfRangeException(nameof(severity))
    };

    public static int Calculate(IReadOnlyCollection<RiskFinding> findings)
    {
        if (findings.Count == 0) return 0;

        var highest = findings.Max(f => f.Severity);
        var score = BaseScore(highest) + (findings.Count - 1) * PointsPerExtraFinding;
        return Math.Min(score, 100);
    }
}
