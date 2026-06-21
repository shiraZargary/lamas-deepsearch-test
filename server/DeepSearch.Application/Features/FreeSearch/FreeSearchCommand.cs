using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Application.Services;
using DeepSearch.Domain.Queries;
using FluentValidation;
using MediatR;

namespace DeepSearch.Application.Features.FreeSearch;

/// <summary>Interprets a free-text question into a structured definition (no execution).</summary>
public sealed record FreeSearchCommand(string Text) : IRequest<FreeSearchResponse>;

/// <summary>
/// The interpretation shown to the user: the structured definition, the readable
/// Hebrew phrasing of it, and notes explaining how the text was understood.
/// </summary>
public sealed record FreeSearchResponse(
    QueryDefinition Definition,
    string Question,
    IReadOnlyList<string> Notes,
    bool Recognized);

public sealed class FreeSearchCommandValidator : AbstractValidator<FreeSearchCommand>
{
    public FreeSearchCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("יש להזין שאלה")
            .MaximumLength(500).WithMessage("השאלה ארוכה מדי");
    }
}

public sealed class FreeSearchCommandHandler(IFreeSearchService freeSearch, QuestionPhrasingService phrasing)
    : IRequestHandler<FreeSearchCommand, FreeSearchResponse>
{
    private readonly IFreeSearchService _freeSearch = freeSearch;
    private readonly QuestionPhrasingService _phrasing = phrasing;

    public async Task<FreeSearchResponse> Handle(FreeSearchCommand request, CancellationToken cancellationToken)
    {
        var result = await _freeSearch.InterpretAsync(request.Text, cancellationToken);
        var question = _phrasing.Phrase(result.Definition);
        return new FreeSearchResponse(result.Definition, question, result.Notes, result.Recognized);
    }
}
