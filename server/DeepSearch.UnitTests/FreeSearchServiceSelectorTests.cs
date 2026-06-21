using DeepSearch.Application.Common.Interfaces;
using DeepSearch.Infrastructure.FreeSearch;

namespace DeepSearch.UnitTests;

public class FreeSearchServiceSelectorTests
{
    private sealed class StubFreeSearch(string name) : IFreeSearchService
    {
        public string Name { get; } = name;
        public Task<FreeSearchResult> InterpretAsync(string question, CancellationToken ct) =>
            throw new NotImplementedException();
    }

    private readonly StubFreeSearch _ruleBased = new("rule");
    private readonly StubFreeSearch _gemini = new("gemini");

    [Theory]
    [InlineData("Gemini", "gemini")]
    [InlineData("gemini", "gemini")]
    [InlineData("RuleBased", "rule")]
    [InlineData("", "rule")]
    [InlineData(null, "rule")]
    [InlineData("anything-else", "rule")]
    public void Select_ReturnsExpectedService(string? provider, string expected)
    {
        var selected = (StubFreeSearch)FreeSearchServiceSelector.Select(provider, _ruleBased, _gemini);
        Assert.Equal(expected, selected.Name);
    }
}
