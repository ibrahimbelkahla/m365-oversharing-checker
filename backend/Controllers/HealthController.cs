using Microsoft.AspNetCore.Mvc;

namespace OversharingChecker.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    public sealed record HealthResponse(string Status);

    [HttpGet]
    public HealthResponse Get() => new("ok");
}
