using DeepSearch.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DeepSearch.Api.Controllers;

/// <summary>Liveness/connectivity checks.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class HealthController(MongoContext context, ILogger<HealthController> logger)
    : ControllerBase
{
    private readonly MongoContext _context = context;
    private readonly ILogger<HealthController> _logger = logger;

    /// <summary>Pings MongoDB and reports connectivity.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        try
        {
            await _context.StatisticsFact.Database
                .RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: ct);
            return Ok(new { status = "healthy", mongo = "connected" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB connectivity check failed");
            return StatusCode(503, new { status = "degraded", mongo = "unreachable" });
        }
    }
}
