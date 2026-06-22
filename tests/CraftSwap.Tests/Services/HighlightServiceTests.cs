using FluentAssertions;
using CraftSwap.Services;

namespace CraftSwap.Tests.Services;

public class HighlightServiceTests
{
    private readonly HighlightService _highlightService;

    public HighlightServiceTests()
    {
        _highlightService = new HighlightService();
    }

    [Fact]
    public void Highlight_WhenTextContainsKeyword_ShouldReturnHighlightedText()
    {
        var text = "红色羊毛线，优质纯羊毛";
        var keyword = "羊毛";

        var result = _highlightService.Highlight(text, keyword);

        result.Should().NotBeNull();
        result.Should().Contain("<em>羊毛</em>");
        result.Should().Be("红色<em>羊毛</em>线，优质纯<em>羊毛</em>");
    }

    [Fact]
    public void Highlight_WhenTextDoesNotContainKeyword_ShouldReturnNull()
    {
        var text = "红色羊毛线，优质纯羊毛";
        var keyword = "棉花";

        var result = _highlightService.Highlight(text, keyword);

        result.Should().BeNull();
    }

    [Fact]
    public void Highlight_WhenKeywordIsNull_ShouldReturnNull()
    {
        var text = "红色羊毛线";
        string? keyword = null;

        var result = _highlightService.Highlight(text, keyword);

        result.Should().BeNull();
    }

    [Fact]
    public void Highlight_WhenTextIsNull_ShouldReturnNull()
    {
        string? text = null;
        var keyword = "羊毛";

        var result = _highlightService.Highlight(text, keyword);

        result.Should().BeNull();
    }

    [Fact]
    public void Highlight_ShouldBeCaseInsensitive()
    {
        var text = "Red Wool Yarn";
        var keyword = "wool";

        var result = _highlightService.Highlight(text, keyword);

        result.Should().NotBeNull();
        result.Should().Contain("<em>Wool</em>");
    }

    [Fact]
    public void Highlight_WithCustomTags_ShouldUseCustomTags()
    {
        var text = "红色羊毛线";
        var keyword = "羊毛";
        var preTag = "<span class=\"highlight\">";
        var postTag = "</span>";

        var result = _highlightService.Highlight(text, keyword, preTag, postTag);

        result.Should().NotBeNull();
        result.Should().Contain("<span class=\"highlight\">羊毛</span>");
    }

    [Fact]
    public void HighlightTags_WhenTagsContainKeyword_ShouldReturnHighlightedTags()
    {
        var tags = new List<string> { "羊毛", "红色", "毛线", "保暖" };
        var keyword = "羊毛";

        var result = _highlightService.HighlightTags(tags, keyword);

        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        result![0].Should().Be("<em>羊毛</em>");
        result[1].Should().Be("红色");
    }

    [Fact]
    public void HighlightTags_WhenNoTagsMatchKeyword_ShouldReturnNull()
    {
        var tags = new List<string> { "红色", "毛线", "保暖" };
        var keyword = "丝绸";

        var result = _highlightService.HighlightTags(tags, keyword);

        result.Should().BeNull();
    }

    [Fact]
    public void HighlightTags_WhenTagsIsNull_ShouldReturnNull()
    {
        List<string>? tags = null;
        var keyword = "羊毛";

        var result = _highlightService.HighlightTags(tags, keyword);

        result.Should().BeNull();
    }

    [Fact]
    public void Highlight_WithSpecialCharactersInKeyword_ShouldHandleCorrectly()
    {
        var text = "价格：$100.50";
        var keyword = "$100";

        var result = _highlightService.Highlight(text, keyword);

        result.Should().NotBeNull();
        result.Should().Contain("<em>$100</em>");
    }
}
