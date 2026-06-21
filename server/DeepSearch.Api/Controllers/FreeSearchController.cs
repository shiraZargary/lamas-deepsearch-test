using DeepSearch.Application.Features.FreeSearch;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeepSearch.Api.Controllers;

/// <summary>Free-text ("free search") querying.</summary>
[ApiController]
[Route("api/free-search")]
public sealed class FreeSearchController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    /// <summary>
    /// Interprets a free-text Hebrew question into a structured definition and its
    /// readable phrasing. Does NOT execute — the client shows the interpretation,
    /// then calls /api/queries/execute to run it.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Search([FromBody] FreeSearchRequest request, CancellationToken ct)
        => Ok(await _sender.Send(new FreeSearchCommand(request.Text), ct));
}

/// <summary>Request body for the free-search endpoint.</summary>
public sealed record FreeSearchRequest(string Text);
