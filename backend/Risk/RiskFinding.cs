using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk;

/// <summary>One problem a rule found on an item.</summary>
/// <param name="RuleId">Stable id of the rule, e.g. "anyone-link".</param>
/// <param name="Reason">Short technical explanation of what was detected.</param>
public sealed record RiskFinding(string RuleId, Severity Severity, string Title, string Reason);
