using OversharingChecker.Api.Risk;

namespace OversharingChecker.Api.Explanations;

/// <summary>Explains an item's risks in plain language and suggests how to fix them.</summary>
public interface IRiskExplainer
{
    /// <summary>Shown in the UI so users know whether AI or templates produced the text.</summary>
    string Name { get; }

    Task<RiskExplanation> ExplainAsync(RiskAssessment assessment, CancellationToken cancellationToken = default);
}

/// <param name="Explanation">What is wrong, for a non-technical employee.</param>
/// <param name="BusinessRisk">Why it matters for the company.</param>
/// <param name="FixSteps">2-3 concrete steps to fix it.</param>
/// <param name="GeneratedBy">Which explainer produced this text.</param>
public sealed record RiskExplanation(string Explanation, string BusinessRisk, IReadOnlyList<string> FixSteps, string GeneratedBy);
