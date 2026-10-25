using System.Text;
using OversharingChecker.Api.Models;
using OversharingChecker.Api.Risk;
using OversharingChecker.Api.Risk.Rules;

namespace OversharingChecker.Api.Explanations;

/// <summary>
/// Builds the prompt for the language model. For privacy it contains metadata only:
/// file name, label, location, permission counts and finding titles - never names or email addresses.
/// </summary>
public static class ExplanationPrompt
{
    public const string SystemMessage =
        "You are a Microsoft 365 security advisor. You explain file-sharing risks to non-technical employees " +
        "in short, friendly, plain English. Reply only with a JSON object with these properties: " +
        "\"explanation\" (2-3 sentences: what is wrong), \"businessRisk\" (1-2 sentences: why it matters for the company), " +
        "\"fixSteps\" (array of 2-3 short, concrete steps the file owner can take in SharePoint, OneDrive or Teams).";

    public static string BuildUserMessage(RiskAssessment assessment)
    {
        var item = assessment.Item;
        var p = item.Permissions;

        var sb = new StringBuilder();
        sb.AppendLine($"File: {item.Name}");
        sb.AppendLine($"Location: {item.LocationType}");
        sb.AppendLine($"Sensitivity label: {LabelText.Of(item.SensitivityLabel)}");
        sb.AppendLine("Permissions:");
        sb.AppendLine($"- Anyone links: {p.Count(x => x.Type == PermissionType.AnyoneLink)} (with edit rights: {p.Count(x => x.Type == PermissionType.AnyoneLink && x.CanEdit)})");
        sb.AppendLine($"- Organization-wide links: {p.Count(x => x.Type == PermissionType.OrganizationLink)}");
        sb.AppendLine($"- Links without expiration: {p.Count(x => x.IsSharingLink && x.ExpirationDate is null)}");
        sb.AppendLine($"- Internal people with direct access: {p.Count(x => x.Type == PermissionType.SpecificPeople && !x.IsExternalGuest)}");
        sb.AppendLine($"- External guests: {p.Count(x => x.IsExternalGuest)} (with edit rights: {p.Count(x => x.IsExternalGuest && x.CanEdit)})");
        sb.AppendLine($"Risk score: {assessment.RiskScore}/100");
        sb.AppendLine("Findings:");
        foreach (var f in assessment.Findings)
            sb.AppendLine($"- [{f.Severity}] {f.Title}");
        if (assessment.Findings.Count == 0)
            sb.AppendLine("- none");

        return sb.ToString();
    }
}
