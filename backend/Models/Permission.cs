namespace OversharingChecker.Api.Models;

/// <summary>One way somebody can access a <see cref="SharedItem"/>.</summary>
public sealed record Permission
{
    public required string Id { get; init; }
    public required PermissionType Type { get; init; }
    public required PermissionRole Role { get; init; }

    /// <summary>Display name of the person, or a description for links (e.g. "Anyone with the link").</summary>
    public required string Grantee { get; init; }

    /// <summary>Email of the person. Null for anyone / organization links.</summary>
    public string? GranteeEmail { get; init; }

    /// <summary>True when the grantee is outside the company (a guest from another domain).</summary>
    public bool IsExternalGuest { get; init; }

    /// <summary>When the sharing link was created. Null for direct grants.</summary>
    public DateTimeOffset? LinkCreated { get; init; }

    /// <summary>When the link stops working. Null means it never expires.</summary>
    public DateTimeOffset? ExpirationDate { get; init; }

    /// <summary>True for anyone / organization links, which work for whoever holds the URL.</summary>
    public bool IsSharingLink => Type is PermissionType.AnyoneLink or PermissionType.OrganizationLink;

    /// <summary>True when the grantee can change the file (write or owner).</summary>
    public bool CanEdit => Role is PermissionRole.Write or PermissionRole.Owner;
}
