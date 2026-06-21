using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Domain.Entities;
using DeepSearch.Domain.Queries;
using FluentValidation;
using MediatR;

namespace DeepSearch.Application.Features.SavedQueries;

/// <summary>Persists a named query definition.</summary>
public sealed record SaveQueryCommand(string Name, QueryDefinition Definition)
    : IRequest<SavedQueryDto>;

public sealed class SaveQueryCommandValidator : AbstractValidator<SaveQueryCommand>
{
    public SaveQueryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("יש להזין שם לשאילתה")
            .MaximumLength(120).WithMessage("שם השאילתה ארוך מדי");
    }
}

public sealed class SaveQueryCommandHandler(ISavedQueryRepository repository)
    : IRequestHandler<SaveQueryCommand, SavedQueryDto>
{
    private readonly ISavedQueryRepository _repository = repository;

    public async Task<SavedQueryDto> Handle(SaveQueryCommand request, CancellationToken cancellationToken)
    {
        var entity = new SavedQuery
        {
            Name = request.Name,
            Definition = request.Definition,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _repository.AddAsync(entity, cancellationToken);
        return new SavedQueryDto(saved.Id, saved.Name, saved.Definition, saved.CreatedAt);
    }
}
