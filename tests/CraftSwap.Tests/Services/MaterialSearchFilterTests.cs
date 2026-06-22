using FluentAssertions;
using CraftSwap.DTOs.Materials;
using CraftSwap.Services;

namespace CraftSwap.Tests.Services;

public class MaterialSearchFilterTests
{
    [Fact]
    public void Parse_WithDefaultParameters_ShouldHaveDefaultValues()
    {
        var parameters = new MaterialQueryParameters();

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.PageNumber.Should().Be(1);
        filter.PageSize.Should().Be(10);
        filter.SearchKeyword.Should().BeNull();
        filter.Categories.Should().BeEmpty();
        filter.Tags.Should().BeEmpty();
        filter.SortBy.Should().Be("CreatedAt");
        filter.SortDirection.Should().Be("desc");
        filter.EnableHighlight.Should().BeTrue();
        filter.SortByRelevance.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithPaginationParameters_ShouldNormalizeValues()
    {
        var parameters = new MaterialQueryParameters
        {
            PageNumber = -5,
            PageSize = 200
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.PageNumber.Should().Be(1);
        filter.PageSize.Should().Be(100);
    }

    [Fact]
    public void Parse_WithSingleCategory_ShouldParseToCategories()
    {
        var parameters = new MaterialQueryParameters
        {
            Category = "毛线"
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.Categories.Should().ContainSingle();
        filter.Categories.Should().Contain("毛线");
    }

    [Fact]
    public void Parse_WithMultipleCategories_ShouldParseAllCategories()
    {
        var parameters = new MaterialQueryParameters
        {
            Categories = "毛线,布料,配件"
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.Categories.Should().HaveCount(3);
        filter.Categories.Should().Contain(new[] { "毛线", "布料", "配件" });
    }

    [Fact]
    public void Parse_WithBothSingleAndMultipleCategories_ShouldCombineAndDedupe()
    {
        var parameters = new MaterialQueryParameters
        {
            Category = "毛线",
            Categories = "毛线,布料,配件"
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.Categories.Should().HaveCount(3);
        filter.Categories.Should().Contain(new[] { "毛线", "布料", "配件" });
    }

    [Fact]
    public void Parse_WithSingleTag_ShouldParseToTags()
    {
        var parameters = new MaterialQueryParameters
        {
            Tag = "羊毛"
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.Tags.Should().ContainSingle();
        filter.Tags.Should().Contain("羊毛");
    }

    [Fact]
    public void Parse_WithMultipleTags_ShouldParseAllTags()
    {
        var parameters = new MaterialQueryParameters
        {
            Tags = "羊毛,红色,保暖"
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.Tags.Should().HaveCount(3);
        filter.Tags.Should().Contain(new[] { "羊毛", "红色", "保暖" });
    }

    [Fact]
    public void Parse_WithDateRange_ShouldParseCorrectly()
    {
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        var parameters = new MaterialQueryParameters
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.StartDate.Should().Be(startDate);
        filter.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void Parse_WithInvalidDateRange_ShouldSwapDates()
    {
        var startDate = new DateTime(2024, 12, 31);
        var endDate = new DateTime(2024, 1, 1);

        var parameters = new MaterialQueryParameters
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.StartDate.Should().Be(endDate);
        filter.EndDate.Should().Be(startDate);
    }

    [Fact]
    public void Parse_WithViewCountRange_ShouldParseCorrectly()
    {
        var parameters = new MaterialQueryParameters
        {
            MinViewCount = 50,
            MaxViewCount = 200
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.MinViewCount.Should().Be(50);
        filter.MaxViewCount.Should().Be(200);
    }

    [Fact]
    public void Parse_WithNegativeMinViewCount_ShouldSetToZero()
    {
        var parameters = new MaterialQueryParameters
        {
            MinViewCount = -10
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.MinViewCount.Should().Be(0);
    }

    [Fact]
    public void Parse_WithNegativeMaxViewCount_ShouldSetToNull()
    {
        var parameters = new MaterialQueryParameters
        {
            MaxViewCount = -5
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.MaxViewCount.Should().BeNull();
    }

    [Fact]
    public void Parse_WithInvalidViewCountRange_ShouldSwapValues()
    {
        var parameters = new MaterialQueryParameters
        {
            MinViewCount = 200,
            MaxViewCount = 50
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.MinViewCount.Should().Be(50);
        filter.MaxViewCount.Should().Be(200);
    }

    [Fact]
    public void Parse_WithFavoriteCountRange_ShouldParseCorrectly()
    {
        var parameters = new MaterialQueryParameters
        {
            MinFavoriteCount = 10,
            MaxFavoriteCount = 50
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.MinFavoriteCount.Should().Be(10);
        filter.MaxFavoriteCount.Should().Be(50);
    }

    [Fact]
    public void Parse_WithSearchKeyword_ShouldSetHasSearchKeywordTrue()
    {
        var parameters = new MaterialQueryParameters
        {
            SearchKeyword = "  羊毛  "
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.SearchKeyword.Should().Be("羊毛");
        filter.HasSearchKeyword.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithEmptySearchKeyword_ShouldSetHasSearchKeywordFalse()
    {
        var parameters = new MaterialQueryParameters
        {
            SearchKeyword = "   "
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.SearchKeyword.Should().BeNull();
        filter.HasSearchKeyword.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithSortByRelevanceTrue_ShouldSetShouldSortByRelevanceTrue()
    {
        var parameters = new MaterialQueryParameters
        {
            SearchKeyword = "羊毛",
            SortByRelevance = true
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.ShouldSortByRelevance.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithoutKeyword_ShouldSetShouldSortByRelevanceFalse()
    {
        var parameters = new MaterialQueryParameters
        {
            SortByRelevance = true
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.ShouldSortByRelevance.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithValidSortBy_ShouldParseCorrectly()
    {
        var parameters = new MaterialQueryParameters
        {
            SortBy = "ViewCount",
            SortDirection = "asc"
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.SortBy.Should().Be("ViewCount");
        filter.SortDirection.Should().Be("asc");
    }

    [Fact]
    public void Parse_WithInvalidSortBy_ShouldUseDefault()
    {
        var parameters = new MaterialQueryParameters
        {
            SortBy = "InvalidField"
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.SortBy.Should().Be("CreatedAt");
    }

    [Fact]
    public void Parse_WithHighlightOptions_ShouldParseCorrectly()
    {
        var parameters = new MaterialQueryParameters
        {
            EnableHighlight = false,
            HighlightPreTag = "<b>",
            HighlightPostTag = "</b>"
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.EnableHighlight.Should().BeFalse();
        filter.HighlightPreTag.Should().Be("<b>");
        filter.HighlightPostTag.Should().Be("</b>");
    }

    [Fact]
    public void Parse_WithOwnerId_ShouldParseCorrectly()
    {
        var parameters = new MaterialQueryParameters
        {
            OwnerId = "123"
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.OwnerId.Should().Be(123);
    }

    [Fact]
    public void Parse_WithInvalidOwnerId_ShouldBeNull()
    {
        var parameters = new MaterialQueryParameters
        {
            OwnerId = "abc"
        };

        var filter = MaterialSearchFilter.Parse(parameters);

        filter.OwnerId.Should().BeNull();
    }
}
