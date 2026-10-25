using OversharingChecker.Api.Data;
using OversharingChecker.Api.Models;

namespace OversharingChecker.Tests.Data;

public class GraphItemMapperTests
{
    private static readonly GraphTenant Tenant = new("Alpenwerk GmbH", "alpenwerk.example");

    [Fact]
    public void Anonymous_edit_link_becomes_anyone_link_with_write_role()
    {
        var graph = new GraphPermission("p1", ["write"], new GraphSharingLink("anonymous", "edit"), null,
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"), null);

        var permission = GraphItemMapper.ToPermission(graph, Tenant);

        Assert.Equal(PermissionType.AnyoneLink, permission.Type);
        Assert.Equal(PermissionRole.Write, permission.Role);
        Assert.NotNull(permission.LinkCreated);
        Assert.Null(permission.ExpirationDate);
    }

    [Theory]
    [InlineData("lena.huber@alpenwerk.example", false)]
    [InlineData("LENA.HUBER@ALPENWERK.EXAMPLE", false)]
    [InlineData("max@freemail.example", true)]
    [InlineData("someone@alpenwerk.example.attacker.example", true)]
    public void Grantee_outside_the_tenant_domain_is_external(string email, bool expectedExternal)
    {
        var graph = new GraphPermission("p1", ["read"], null,
            new GraphIdentitySet(new GraphIdentity("Someone", email)), null, null);

        var permission = GraphItemMapper.ToPermission(graph, Tenant);

        Assert.Equal(PermissionType.SpecificPeople, permission.Type);
        Assert.Equal(expectedExternal, permission.IsExternalGuest);
    }

    [Fact]
    public void Label_display_name_with_space_is_parsed()
    {
        var graph = new GraphDriveItem("i1", "Plan.xlsx", "Teams", DateTimeOffset.UtcNow,
            new GraphIdentitySet(new GraphIdentity("Owner", "owner@alpenwerk.example")),
            new GraphParentReference("Projekt", "/sites/Projekt/Shared Documents"),
            new GraphSensitivityLabel("Highly Confidential"), null);

        var item = GraphItemMapper.ToSharedItem(graph, Tenant);

        Assert.Equal(SensitivityLabel.HighlyConfidential, item.SensitivityLabel);
        Assert.Equal(LocationType.Teams, item.LocationType);
        Assert.Equal("/sites/Projekt/Shared Documents/Plan.xlsx", item.Path);
        Assert.Empty(item.Permissions);
    }
}
