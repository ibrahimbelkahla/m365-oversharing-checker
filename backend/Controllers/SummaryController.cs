using Microsoft.AspNetCore.Mvc;
using OversharingChecker.Api.Services;

namespace OversharingChecker.Api.Controllers;

[ApiController]
[Route("api/summary")]
public sealed class SummaryController(ScanService scanService) : ControllerBase
{
    /// <summary>Totals per severity, external shares and the riskiest sites/teams.</summary>
    [HttpGet]
    public Task<ScanSummary> GetSummary(CancellationToken cancellationToken) =>
        scanService.GetSummaryAsync(cancellationToken);
}
