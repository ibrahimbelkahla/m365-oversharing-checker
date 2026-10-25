using Microsoft.AspNetCore.Mvc;
using OversharingChecker.Api.Models;
using OversharingChecker.Api.Risk;
using OversharingChecker.Api.Services;

namespace OversharingChecker.Api.Controllers;

[ApiController]
[Route("api/items")]
public sealed class ItemsController(ScanService scanService) : ControllerBase
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
}
