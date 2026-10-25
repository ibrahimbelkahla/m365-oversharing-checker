using Microsoft.AspNetCore.Mvc;
using OversharingChecker.Api.Explanations;
using OversharingChecker.Api.Models;
using OversharingChecker.Api.Risk;
using OversharingChecker.Api.Services;

namespace OversharingChecker.Api.Controllers;

[ApiController]
[Route("api/items")]
public sealed class ItemsController(ScanService scanService, IRiskExplainer explainer, ILogger<ItemsController> logger)
    : ControllerBase
{
    /// <summary>All scanned items with findings and scores, riskiest first.</summary>
    [HttpGet]
    public async Task<IReadOnlyList<RiskAssessment>> GetItems(
        [FromQuery] Severity? severity,
        [FromQuery] LocationType? location,
        [FromQuery] SensitivityLabel? label,
        CancellationToken cancellationToken) =>
        await scanService.GetAssessmentsAsync(new ItemFilter(severity, location, label), cancellationToken);

    /// <summary>One item with its permissions and findings.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RiskAssessment>> GetItem(string id, CancellationToken cancellationToken)
    {
        var result = await scanService.GetAssessmentAsync(id, cancellationToken);
        return result is null ? NotFound() : result;
    }

    /// <summary>Plain-language explanation of the item's risks with suggested fixes.</summary>
    [HttpPost("{id}/explain")]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<RiskExplanation>> Explain(string id, CancellationToken cancellationToken)
    {
        var assessment = await scanService.GetAssessmentAsync(id, cancellationToken);
        if (assessment is null) return NotFound();

        try
        {
            return await explainer.ExplainAsync(assessment, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Network errors, timeouts, invalid AI answers: log the details, return a clean error to the client.
            logger.LogWarning(ex, "Explanation for item {ItemId} failed", id);
            return Problem("The AI service could not create an explanation. Please try again later.",
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
