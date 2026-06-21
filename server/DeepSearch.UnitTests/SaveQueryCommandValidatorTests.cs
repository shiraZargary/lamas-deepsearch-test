using DeepSearch.Application.Features.SavedQueries;
using DeepSearch.Domain.Queries;

namespace DeepSearch.UnitTests;

public class SaveQueryCommandValidatorTests
{
    private readonly SaveQueryCommandValidator _validator = new();

    [Fact]
    public void EmptyName_Fails()
    {
        var cmd = new SaveQueryCommand("", new QueryDefinition());
        Assert.False(_validator.Validate(cmd).IsValid);
    }

    [Fact]
    public void ValidName_Passes()
    {
        var cmd = new SaveQueryCommand("שכר נשים ירושלים", new QueryDefinition());
        Assert.True(_validator.Validate(cmd).IsValid);
    }
}
