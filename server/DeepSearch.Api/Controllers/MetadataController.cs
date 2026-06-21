using DeepSearch.Application.Features.Metadata;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeepSearch.Api.Controllers;

/// <summary>Exposes the metadata that drives the query-builder UI.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class MetadataController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    /// <summary>Returns the available metrics and dimensions.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await _sender.Send(new GetMetadataQuery(), ct));
}
