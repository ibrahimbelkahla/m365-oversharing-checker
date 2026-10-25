using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Risk;

/// <summary>A single oversharing check. Each rule returns at most one finding per item.</summary>
public interface IRiskRule
{
    /// <returns>A finding when the item breaks the rule, otherwise null.</returns>
    RiskFinding? Evaluate(SharedItem item);
}
