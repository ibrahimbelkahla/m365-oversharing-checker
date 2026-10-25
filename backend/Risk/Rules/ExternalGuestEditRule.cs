using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk.Rules;

/// <summary>Rule 2: a guest from outside the company can change or delete the file.</summary>
public sealed class ExternalGuestEditRule : IRiskRule
{
    public const string Id = "external-guest-edit";

    public RiskFinding? Evaluate(SharedItem item)
    {
        var guests = item.Permissions.Where(p => p.IsExternalGuest && p.CanEdit).ToList();
        if (guests.Count == 0) return null;

        return new RiskFinding(Id, Severity.High, "External guest can edit",
            $"External user(s) with edit rights: {string.Join(", ", guests.Select(g => g.GranteeEmail))}.");
    }
}
