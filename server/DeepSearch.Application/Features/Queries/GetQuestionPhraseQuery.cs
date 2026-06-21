using DeepSearch.Application.Services;
using DeepSearch.Domain.Queries;
using MediatR;

namespace DeepSearch.Application.Features.Queries;

/// <summary>Returns only the readable Hebrew phrasing for a definition (no DB hit).</summary>
public sealed record GetQuestionPhraseQuery(QueryDefinition Definition) : IRequest<string>;

public sealed class GetQuestionPhraseQueryHandler(QuestionPhrasingService phrasing)
    : IRequestHandler<GetQuestionPhraseQuery, string>
{
    private readonly QuestionPhrasingService _phrasing = phrasing;

    public Task<string> Handle(GetQuestionPhraseQuery request, CancellationToken cancellationToken)
        => Task.FromResult(_phrasing.Phrase(request.Definition));
}
