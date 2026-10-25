using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk.Rules;

/// <summary>Rule 4: a "People in your organization" link on a Confidential or Highly Confidential file.</summary>
public sealed class OrganizationLinkOnConfidentialRule : IRiskRule
{
    public const string Id = "org-link-confidential";

    public RiskFinding? Evaluate(SharedItem item)
    {
        if (item.SensitivityLabel < SensitivityLabel.Confidential) return null;
        if (!item.Permissions.Any(p => p.Type == PermissionType.OrganizationLink)) return null;

        var severity = item.SensitivityLabel == SensitivityLabel.HighlyConfidential ? Severity.High : Severity.Medium;

        return new RiskFinding(Id, severity, "Organization-wide link on sensitive file",
            $"Every employee with the link can open this {LabelText.Of(item.SensitivityLabel)} file, " +
            "not only the people who need it.");
    }
}
