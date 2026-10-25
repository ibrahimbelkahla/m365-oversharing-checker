using Microsoft.AspNetCore.Mvc;
using OversharingChecker.Api.Explanations;

namespace OversharingChecker.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(IRiskExplainer explainer) : ControllerBase
{
    /// <param name="Explainer">Name of the active explainer, shown in the UI.</param>
    /// <param name="AiEnabled">True when a language model (not templates) produces explanations.</param>
    public sealed record HealthResponse(string Status, string Explainer, bool AiEnabled);

    [HttpGet]
    public HealthResponse Get() => new("ok", explainer.Name, explainer is not RuleBasedRiskExplainer);
}
