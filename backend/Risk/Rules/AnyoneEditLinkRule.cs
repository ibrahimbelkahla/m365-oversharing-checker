using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk.Rules;

/// <summary>Rule 7: anyone with the URL can change or delete the file without signing in. Always Critical.</summary>
public sealed class AnyoneEditLinkRule : IRiskRule
{
    public const string Id = "anyone-edit-link";

    public RiskFinding? Evaluate(SharedItem item)
    {
        var links = item.Permissions.Count(p => p.Type == PermissionType.AnyoneLink && p.CanEdit);
        if (links == 0) return null;

        return new RiskFinding(Id, Severity.Critical, "Anonymous link with edit rights",
            $"{links} link(s) let anyone with the URL edit, overwrite or delete the file without signing in.");
    }
}
