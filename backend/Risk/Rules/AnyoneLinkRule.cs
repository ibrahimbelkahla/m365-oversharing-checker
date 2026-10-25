using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk.Rules;

/// <summary>
/// Rule 1: an "Anyone" link lets people open the file without signing in.
/// The more sensitive the file, the higher the severity. Edit links are handled by <see cref="AnyoneEditLinkRule"/>.
/// </summary>
public sealed class AnyoneLinkRule : IRiskRule
{
    public const string Id = "anyone-link";

    public RiskFinding? Evaluate(SharedItem item)
    {
        var links = item.Permissions.Count(p => p.Type == PermissionType.AnyoneLink && !p.CanEdit);
        if (links == 0) return null;

        var severity = item.SensitivityLabel switch
        {
            SensitivityLabel.HighlyConfidential => Severity.Critical,
            SensitivityLabel.Confidential => Severity.High,
            SensitivityLabel.Internal => Severity.Medium,
            _ => Severity.Low
        };

        return new RiskFinding(Id, severity, "Anonymous \"Anyone\" link",
            $"{links} view link(s) can be opened by anyone who has the URL, without signing in. " +
            $"The file is labeled {LabelText.Of(item.SensitivityLabel)}.");
    }
}
