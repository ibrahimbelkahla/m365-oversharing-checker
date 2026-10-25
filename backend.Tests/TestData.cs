using OversharingChecker.Api.Models;

namespace OversharingChecker.Tests;

/// <summary>Small helpers to build items and permissions in tests.</summary>
public static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

    private static int _counter;
    private static string NextId() => $"p{Interlocked.Increment(ref _counter)}";

    public static SharedItem Item(SensitivityLabel label, params Permission[] permissions) => new()
    {
        Id = "item-1",
        Name = "Test.xlsx",
        Path = "/sites/Test/Shared Documents/Test.xlsx",
        Site = "Test",
        LocationType = LocationType.SharePoint,
        SensitivityLabel = label,
        Owner = "Owner",
        LastModified = Now,
        Permissions = permissions
    };

    public static Permission AnyoneLink(PermissionRole role = PermissionRole.Read, int createdDaysAgo = 1, int? expiresInDays = 30) =>
        Link(PermissionType.AnyoneLink, role, createdDaysAgo, expiresInDays);

    public static Permission OrgLink(PermissionRole role = PermissionRole.Read, int createdDaysAgo = 1, int? expiresInDays = 30) =>
        Link(PermissionType.OrganizationLink, role, createdDaysAgo, expiresInDays);

    public static Permission Internal(PermissionRole role = PermissionRole.Read, string name = "colleague") => new()
    {
        Id = NextId(),
        Type = PermissionType.SpecificPeople,
        Role = role,
        Grantee = name,
        GranteeEmail = $"{name}@alpenwerk.example"
    };

    public static Permission External(PermissionRole role = PermissionRole.Read) => new()
    {
        Id = NextId(),
        Type = PermissionType.SpecificPeople,
        Role = role,
        Grantee = "Guest",
        GranteeEmail = "guest@partner.example",
        IsExternalGuest = true
    };

    private static Permission Link(PermissionType type, PermissionRole role, int createdDaysAgo, int? expiresInDays) => new()
    {
        Id = NextId(),
        Type = type,
        Role = role,
        Grantee = type.ToString(),
        LinkCreated = Now.AddDays(-createdDaysAgo),
        ExpirationDate = expiresInDays is { } days ? Now.AddDays(days) : null
    };
}

/// <summary>A clock frozen at a fixed moment, so date-based tests never break.</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
