using OversharingChecker.Api.Models;
using OversharingChecker.Api.Risk.Rules;
using static OversharingChecker.Tests.TestData;

namespace OversharingChecker.Tests.Risk;

public class AnyoneLinkRuleTests
{
    private readonly AnyoneLinkRule _rule = new();

    [Theory]
    [InlineData(SensitivityLabel.Public, Severity.Low)]
    [InlineData(SensitivityLabel.Internal, Severity.Medium)]
    [InlineData(SensitivityLabel.Confidential, Severity.High)]
    [InlineData(SensitivityLabel.HighlyConfidential, Severity.Critical)]
    public void Anyone_view_link_severity_depends_on_label(SensitivityLabel label, Severity expected)
    {
        var finding = _rule.Evaluate(Item(label, AnyoneLink()));

        Assert.NotNull(finding);
        Assert.Equal(expected, finding.Severity);
        Assert.Equal(AnyoneLinkRule.Id, finding.RuleId);
    }

    [Fact]
    public void No_finding_without_anyone_link() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.HighlyConfidential, OrgLink(), Internal())));

    [Fact]
    public void Anyone_edit_link_is_left_to_the_edit_rule() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.Internal, AnyoneLink(PermissionRole.Write))));
}

public class ExternalGuestEditRuleTests
{
    private readonly ExternalGuestEditRule _rule = new();

    [Theory]
    [InlineData(PermissionRole.Write)]
    [InlineData(PermissionRole.Owner)]
    public void External_guest_with_edit_rights_is_high(PermissionRole role)
    {
        var finding = _rule.Evaluate(Item(SensitivityLabel.Public, External(role)));

        Assert.NotNull(finding);
        Assert.Equal(Severity.High, finding.Severity);
        Assert.Contains("guest@partner.example", finding.Reason);
    }

    [Fact]
    public void External_guest_with_read_only_is_fine() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.Internal, External(PermissionRole.Read))));

    [Fact]
    public void Internal_user_with_edit_rights_is_fine() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.Internal, Internal(PermissionRole.Write))));
}

public class ConfidentialExternalShareRuleTests
{
    private readonly ConfidentialExternalShareRule _rule = new();

    [Theory]
    [InlineData(SensitivityLabel.Confidential, Severity.High)]
    [InlineData(SensitivityLabel.HighlyConfidential, Severity.Critical)]
    public void Sensitive_file_shared_externally_is_flagged(SensitivityLabel label, Severity expected)
    {
        var finding = _rule.Evaluate(Item(label, External(), Internal()));

        Assert.NotNull(finding);
        Assert.Equal(expected, finding.Severity);
    }

    [Theory]
    [InlineData(SensitivityLabel.Public)]
    [InlineData(SensitivityLabel.Internal)]
    public void Less_sensitive_file_shared_externally_is_not_flagged(SensitivityLabel label) =>
        Assert.Null(_rule.Evaluate(Item(label, External())));

    [Fact]
    public void Sensitive_file_shared_only_internally_is_fine() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.HighlyConfidential, Internal(), Internal())));
}

public class OrganizationLinkOnConfidentialRuleTests
{
    private readonly OrganizationLinkOnConfidentialRule _rule = new();

    [Theory]
    [InlineData(SensitivityLabel.Confidential, Severity.Medium)]
    [InlineData(SensitivityLabel.HighlyConfidential, Severity.High)]
    public void Org_link_on_sensitive_file_is_flagged(SensitivityLabel label, Severity expected)
    {
        var finding = _rule.Evaluate(Item(label, OrgLink()));

        Assert.NotNull(finding);
        Assert.Equal(expected, finding.Severity);
    }

    [Fact]
    public void Org_link_on_internal_file_is_fine() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.Internal, OrgLink())));

    [Fact]
    public void Sensitive_file_without_org_link_is_fine() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.HighlyConfidential, Internal())));
}

public class StaleOrNonExpiringLinkRuleTests
{
    private readonly StaleOrNonExpiringLinkRule _rule = new(new FixedTimeProvider(Now));

    [Fact]
    public void Anyone_link_without_expiration_is_medium()
    {
        var finding = _rule.Evaluate(Item(SensitivityLabel.Public, AnyoneLink(expiresInDays: null)));

        Assert.NotNull(finding);
        Assert.Equal(Severity.Medium, finding.Severity);
        Assert.Contains("no expiration", finding.Reason);
    }

    [Fact]
    public void Org_link_without_expiration_is_low()
    {
        var finding = _rule.Evaluate(Item(SensitivityLabel.Internal, OrgLink(expiresInDays: null)));

        Assert.NotNull(finding);
        Assert.Equal(Severity.Low, finding.Severity);
    }

    [Fact]
    public void Link_older_than_90_days_is_flagged_even_with_expiration()
    {
        var finding = _rule.Evaluate(Item(SensitivityLabel.Internal, OrgLink(createdDaysAgo: 91, expiresInDays: 200)));

        Assert.NotNull(finding);
        Assert.Contains("older than 90 days", finding.Reason);
    }

    [Fact]
    public void Recent_link_with_expiration_is_fine() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.Internal, AnyoneLink(createdDaysAgo: 90, expiresInDays: 30))));

    [Fact]
    public void Direct_grants_are_not_sharing_links() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.Internal, Internal(), External())));
}

public class ExcessiveDirectSharingRuleTests
{
    private readonly ExcessiveDirectSharingRule _rule = new();

    private static Permission[] People(int count) =>
        Enumerable.Range(1, count).Select(i => Internal(name: $"user{i}")).ToArray();

    [Fact]
    public void More_than_25_people_is_medium()
    {
        var finding = _rule.Evaluate(Item(SensitivityLabel.Internal, People(26)));

        Assert.NotNull(finding);
        Assert.Equal(Severity.Medium, finding.Severity);
        Assert.Contains("26 people", finding.Reason);
    }

    [Fact]
    public void Exactly_25_people_is_fine() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.Internal, People(25))));

    [Fact]
    public void Links_do_not_count_as_direct_grants() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.Internal, [.. People(25), OrgLink(), AnyoneLink()])));

    [Fact]
    public void Threshold_is_configurable() =>
        Assert.NotNull(new ExcessiveDirectSharingRule(maxDirectGrants: 2).Evaluate(Item(SensitivityLabel.Internal, People(3))));
}

public class AnyoneEditLinkRuleTests
{
    private readonly AnyoneEditLinkRule _rule = new();

    [Theory]
    [InlineData(SensitivityLabel.Public)]
    [InlineData(SensitivityLabel.HighlyConfidential)]
    public void Anyone_edit_link_is_always_critical(SensitivityLabel label)
    {
        var finding = _rule.Evaluate(Item(label, AnyoneLink(PermissionRole.Write)));

        Assert.NotNull(finding);
        Assert.Equal(Severity.Critical, finding.Severity);
    }

    [Fact]
    public void Anyone_view_link_is_not_an_edit_link() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.Public, AnyoneLink(PermissionRole.Read))));

    [Fact]
    public void Org_edit_link_is_not_anonymous() =>
        Assert.Null(_rule.Evaluate(Item(SensitivityLabel.Public, OrgLink(PermissionRole.Write))));
}
