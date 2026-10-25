using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk.Rules;

/// <summary>Human-readable label names for messages ("Highly Confidential" instead of "HighlyConfidential").</summary>
public static class LabelText
{
    public static string Of(SensitivityLabel label) =>
        label == SensitivityLabel.HighlyConfidential ? "Highly Confidential" : label.ToString();
}
