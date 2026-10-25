namespace OversharingChecker.Api.Models;

/// <summary>A file in SharePoint, OneDrive or Teams together with its permissions.</summary>
public sealed record SharedItem
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }

    /// <summary>SharePoint site, Team name, or "OneDrive – owner".</summary>
    public required string Site { get; init; }

    public required LocationType LocationType { get; init; }
    public required SensitivityLabel SensitivityLabel { get; init; }
    public required string Owner { get; init; }
    public required DateTimeOffset LastModified { get; init; }
    public IReadOnlyList<Permission> Permissions { get; init; } = [];
}
