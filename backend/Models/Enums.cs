namespace OversharingChecker.Api.Models;

/// <summary>Where a file lives in Microsoft 365.</summary>
public enum LocationType
{
    SharePoint,
    OneDrive,
    Teams
}

/// <summary>Microsoft Purview sensitivity label, ordered from least to most sensitive.</summary>
public enum SensitivityLabel
{
    Public,
    Internal,
    Confidential,
    HighlyConfidential
}

/// <summary>How access was granted.</summary>
public enum PermissionType
{
    /// <summary>"Anyone with the link" - no sign-in required (anonymous).</summary>
    AnyoneLink,

    /// <summary>"People in your organization" link - every employee who has the link.</summary>
    OrganizationLink,

    /// <summary>Granted to named people directly.</summary>
    SpecificPeople
}

public enum PermissionRole
{
    Read,
    Write,
    Owner
}

/// <summary>Severity of a risk finding, ordered from lowest to highest.</summary>
public enum Severity
{
    Low,
    Medium,
    High,
    Critical
}
