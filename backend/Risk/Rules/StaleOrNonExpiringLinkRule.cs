using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk.Rules;

/// <summary>
/// Rule 5: an Anyone / organization link that never expires, or was created more than 90 days ago.
/// Forgotten links keep working long after the reason for sharing is gone.
/// </summary>
public sealed class StaleOrNonExpiringLinkRule(TimeProvider timeProvider) : IRiskRule
{
    public const string Id = "stale-link";
    public static readonly TimeSpan MaxLinkAge = TimeSpan.FromDays(90);

    public RiskFinding? Evaluate(SharedItem item)
    {
        var now = timeProvider.GetUtcNow();
        var links = item.Permissions.Where(p => p.IsSharingLink).ToList();

        var neverExpire = links.Where(p => p.ExpirationDate is null).ToList();
        var tooOld = links.Where(p => p.LinkCreated is { } created && now - created > MaxLinkAge).ToList();
        if (neverExpire.Count == 0 && tooOld.Count == 0) return null;

        // Anonymous links are more dangerous to forget than organization links.
        var severity = neverExpire.Concat(tooOld).Any(p => p.Type == PermissionType.AnyoneLink)
            ? Severity.Medium
            : Severity.Low;

        var reasons = new List<string>();
        if (neverExpire.Count > 0) reasons.Add($"{neverExpire.Count} link(s) have no expiration date");
        if (tooOld.Count > 0) reasons.Add($"{tooOld.Count} link(s) are older than {MaxLinkAge.Days} days");

        return new RiskFinding(Id, severity, "Sharing link never expires or is old",
            string.Join("; ", reasons) + ".");
    }
}
