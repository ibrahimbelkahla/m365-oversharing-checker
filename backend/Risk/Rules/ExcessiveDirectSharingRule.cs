using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk.Rules;

/// <summary>Rule 6: the file is shared directly with many individual people, which is hard to review and maintain.</summary>
public sealed class ExcessiveDirectSharingRule(int maxDirectGrants = ExcessiveDirectSharingRule.DefaultMaxDirectGrants) : IRiskRule
{
    public const string Id = "excessive-direct-sharing";
    public const int DefaultMaxDirectGrants = 25;

    public RiskFinding? Evaluate(SharedItem item)
    {
        var count = item.Permissions.Count(p => p.Type == PermissionType.SpecificPeople);
        if (count <= maxDirectGrants) return null;

        return new RiskFinding(Id, Severity.Medium, "Shared with many individual people",
            $"Shared directly with {count} people (limit {maxDirectGrants}). A group would be easier to manage.");
    }
}
