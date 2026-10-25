using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk.Rules;

/// <summary>Rule 3: a Confidential or Highly Confidential file is shared with someone outside the company.</summary>
public sealed class ConfidentialExternalShareRule : IRiskRule
{
    public const string Id = "confidential-external-share";

    public RiskFinding? Evaluate(SharedItem item)
    {
        if (item.SensitivityLabel < SensitivityLabel.Confidential) return null;

        var guests = item.Permissions.Where(p => p.IsExternalGuest).ToList();
        if (guests.Count == 0) return null;

        var severity = item.SensitivityLabel == SensitivityLabel.HighlyConfidential ? Severity.Critical : Severity.High;

        return new RiskFinding(Id, severity, "Sensitive file shared externally",
            $"{LabelText.Of(item.SensitivityLabel)} file shared with {guests.Count} external user(s): " +
            $"{string.Join(", ", guests.Select(g => g.GranteeEmail))}.");
    }
}
