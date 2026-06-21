using DeepSearch.Application.Features.Queries;
using DeepSearch.Application.Features.SavedQueries;
using DeepSearch.Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeepSearch.Api.Controllers;

/// <summary>Builds, phrases and executes structured queries.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class QueriesController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    /// <summary>Executes a query definition and returns the phrasing + tabular result.</summary>
    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] QueryDefinition definition, CancellationToken ct)
        => Ok(await _sender.Send(new ExecuteQueryQuery(definition), ct));

    /// <summary>Returns only the readable Hebrew phrasing (for live preview).</summary>
    [HttpPost("phrase")]
    public async Task<IActionResult> Phrase([FromBody] QueryDefinition definition, CancellationToken ct)
        => Ok(new { question = await _sender.Send(new GetQuestionPhraseQuery(definition), ct) });

    /// <summary>Saves a named query definition.</summary>
    [HttpPost("saved")]
    public async Task<IActionResult> Save([FromBody] SaveQueryRequest request, CancellationToken ct)
        => Ok(await _sender.Send(new SaveQueryCommand(request.Name, request.Definition), ct));

    /// <summary>Lists all saved queries (most recent first).</summary>
    [HttpGet("saved")]
    public async Task<IActionResult> GetSaved(CancellationToken ct)
        => Ok(await _sender.Send(new GetSavedQueriesQuery(), ct));

    /// <summary>Re-runs a saved query by id.</summary>
    [HttpPost("saved/{id}/run")]
    public async Task<IActionResult> RunSaved(string id, CancellationToken ct)
        => Ok(await _sender.Send(new RunSavedQueryQuery(id), ct));
}

/// <summary>Request body for saving a query.</summary>
public sealed record SaveQueryRequest(string Name, QueryDefinition Definition);
