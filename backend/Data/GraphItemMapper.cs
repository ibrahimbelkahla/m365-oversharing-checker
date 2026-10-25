using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Data;

/// <summary>Translates Graph-shaped DTOs into our own domain models.</summary>
public static class GraphItemMapper
{
    public static SharedItem ToSharedItem(GraphDriveItem item, GraphTenant tenant) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Path = $"{item.ParentReference.Path}/{item.Name}",
        Site = item.ParentReference.SiteName,
        LocationType = Enum.Parse<LocationType>(item.LocationType, ignoreCase: true),
        SensitivityLabel = ParseLabel(item.SensitivityLabel?.DisplayName),
        Owner = item.CreatedBy.User.DisplayName,
        LastModified = item.LastModifiedDateTime,
        Permissions = (item.Permissions ?? []).Select(p => ToPermission(p, tenant)).ToList()
    };

    public static Permission ToPermission(GraphPermission p, GraphTenant tenant)
    {
        var type = p.Link?.Scope switch
        {
            "anonymous" => PermissionType.AnyoneLink,
            "organization" => PermissionType.OrganizationLink,
            _ => PermissionType.SpecificPeople // "users" links and direct grants
        };

        var email = p.GrantedToV2?.User.Email;

        return new Permission
        {
            Id = p.Id,
            Type = type,
            Role = ParseRole(p),
            Grantee = type switch
            {
                PermissionType.AnyoneLink => "Anyone with the link",
                PermissionType.OrganizationLink => $"Everyone at {tenant.DisplayName}",
                _ => p.GrantedToV2?.User.DisplayName ?? "Unknown"
            },
            GranteeEmail = email,
            IsExternalGuest = email is not null && !IsInternalEmail(email, tenant.Domain),
            LinkCreated = p.Link is null ? null : p.CreatedDateTime,
            ExpirationDate = p.ExpirationDateTime
        };
    }

    /// <summary>An address is internal when its domain is the tenant's domain.</summary>
    public static bool IsInternalEmail(string email, string tenantDomain) =>
        email.EndsWith("@" + tenantDomain, StringComparison.OrdinalIgnoreCase);

    private static PermissionRole ParseRole(GraphPermission p)
    {
        if (p.Roles.Contains("owner")) return PermissionRole.Owner;
        if (p.Roles.Contains("write") || p.Link?.Type == "edit") return PermissionRole.Write;
        return PermissionRole.Read;
    }

    // Labels arrive as display names ("Highly Confidential"); items without a label are treated as Internal.
    private static SensitivityLabel ParseLabel(string? displayName) =>
        displayName is null
            ? SensitivityLabel.Internal
            : Enum.Parse<SensitivityLabel>(displayName.Replace(" ", ""), ignoreCase: true);
}
