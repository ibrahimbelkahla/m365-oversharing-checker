using OversharingChecker.Api.Risk;
using OversharingChecker.Api.Risk.Rules;

namespace OversharingChecker.Api.Explanations;

/// <summary>Builds explanations from fixed text templates. Needs no API key, so the app always works.</summary>
public sealed class RuleBasedRiskExplainer : IRiskExplainer
{
    public const int MaxFixSteps = 3;

    private sealed record Template(string Explanation, string BusinessRisk, string[] FixSteps);

    private static readonly Dictionary<string, Template> Templates = new()
    {
        [AnyoneLinkRule.Id] = new(
            "This file can be opened by anyone who has the link, without logging in. If the link is forwarded or posted somewhere, strangers can read it.",
            "Company data can leak outside the organization, and there is no record of who opened it.",
            ["Replace the \"Anyone\" link with a link for specific people.",
             "If the file really must be public, add an expiration date to the link."]),

        [AnyoneEditLinkRule.Id] = new(
            "Anyone who has the link can change or delete this file without logging in.",
            "Strangers could alter, delete or replace the content (for example with malware), and nobody would know who did it.",
            ["Remove the \"Anyone can edit\" link immediately.",
             "Share the file with specific people and give edit rights only to those who need them."]),

        [ExternalGuestEditRule.Id] = new(
            "Someone outside the company can edit this file.",
            "If the partner's account is misused or hacked, company data can be changed or deleted from outside.",
            ["Change the guest's permission from \"Can edit\" to \"Can view\" unless editing is really needed.",
             "Ask the file owner to confirm that this external access is still required."]),

        [ConfidentialExternalShareRule.Id] = new(
            "A file marked as confidential is shared with people outside the company.",
            "Confidential data at external parties can break NDAs, contracts or data protection rules such as the GDPR.",
            ["Check that an NDA or contract covers this sharing; otherwise remove the external access.",
             "Share a reduced version without the sensitive parts instead."]),

        [OrganizationLinkOnConfidentialRule.Id] = new(
            "Every employee who gets the link can open this confidential file, not only the people who need it.",
            "Sensitive information becomes visible to far too many people, and AI assistants such as Copilot can surface it in their answers.",
            ["Replace the company-wide link with access for a specific group or team.",
             "Review who actually needs this file and remove everyone else."]),

        [StaleOrNonExpiringLinkRule.Id] = new(
            "A sharing link for this file never expires or has been active for a long time.",
            "Forgotten links keep working long after the project is over, so access is never cleaned up.",
            ["Set an expiration date on the link (for example 30 days).",
             "Delete links that are no longer needed."]),

        [ExcessiveDirectSharingRule.Id] = new(
            "This file is shared one by one with a very large number of people.",
            "Nobody can keep track of who has access, and people who changed teams or left keep it.",
            ["Share the file with a Microsoft 365 group or Team instead of individual people.",
             "Remove people who no longer need access."])
    };

    /// <summary>Rule ids that have a template. Used by tests to make sure no rule is forgotten.</summary>
    public static IReadOnlyCollection<string> SupportedRuleIds => Templates.Keys;

    public string Name => "Rule-based templates";

    public Task<RiskExplanation> ExplainAsync(RiskAssessment assessment, CancellationToken cancellationToken = default)
    {
        // Findings are already sorted most severe first, so the first template describes the biggest problem.
        var templates = assessment.Findings
            .Select(f => Templates.GetValueOrDefault(f.RuleId))
            .OfType<Template>()
            .Distinct()
            .ToList();

        if (templates.Count == 0)
        {
            return Task.FromResult(new RiskExplanation(
                $"No oversharing risks were found for \"{assessment.Item.Name}\".",
                "No action is needed right now.",
                [],
                Name));
        }

        return Task.FromResult(new RiskExplanation(
            string.Join(" ", templates.Select(t => t.Explanation)),
            templates[0].BusinessRisk,
            templates.SelectMany(t => t.FixSteps).Distinct().Take(MaxFixSteps).ToList(),
            Name));
    }
}
