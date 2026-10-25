namespace OversharingChecker.Api.Data;

// These classes mirror (a simplified version of) the JSON that Microsoft Graph returns for
// GET /drives/{id}/items/{id}?$expand=permissions. Keeping them separate from our domain models
// means a real Graph data source can reuse the same mapping code.

public sealed record GraphResponse(GraphTenant Tenant, List<GraphDriveItem> Value);

public sealed record GraphTenant(string DisplayName, string Domain);

public sealed record GraphDriveItem(
    string Id,
    string Name,
    string LocationType,
    DateTimeOffset LastModifiedDateTime,
    GraphIdentitySet CreatedBy,
    GraphParentReference ParentReference,
    GraphSensitivityLabel? SensitivityLabel,
    List<GraphPermission>? Permissions);

public sealed record GraphParentReference(string SiteName, string Path);

public sealed record GraphSensitivityLabel(string DisplayName);

public sealed record GraphIdentitySet(GraphIdentity User);

public sealed record GraphIdentity(string DisplayName, string? Email);

public sealed record GraphPermission(
    string Id,
    List<string> Roles,
    GraphSharingLink? Link,
    GraphIdentitySet? GrantedToV2,
    DateTimeOffset? CreatedDateTime,
    DateTimeOffset? ExpirationDateTime);

/// <param name="Scope">"anonymous", "organization" or "users".</param>
/// <param name="Type">"view" or "edit".</param>
public sealed record GraphSharingLink(string Scope, string Type);
