using CraftSwap.DTOs.Materials;

namespace CraftSwap.Services;

/// <summary>
/// 材料搜索条件解析器
/// 负责解析、验证和规范化搜索参数，属于条件解析层
/// </summary>
public class MaterialSearchFilter
{
    /// <summary>
    /// 解析后的分页参数
    /// </summary>
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }

    /// <summary>
    /// 解析后的搜索关键词
    /// </summary>
    public string? SearchKeyword { get; private set; }

    /// <summary>
    /// 解析后的分类列表
    /// </summary>
    public List<string> Categories { get; private set; } = new();

    /// <summary>
    /// 解析后的标签列表
    /// </summary>
    public List<string> Tags { get; private set; } = new();

    /// <summary>
    /// 解析后的状态
    /// </summary>
    public string? Status { get; private set; }

    /// <summary>
    /// 解析后的所有者ID
    /// </summary>
    public int? OwnerId { get; private set; }

    /// <summary>
    /// 解析后的时间区间
    /// </summary>
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// 解析后的浏览量范围
    /// </summary>
    public int? MinViewCount { get; private set; }
    public int? MaxViewCount { get; private set; }

    /// <summary>
    /// 解析后的收藏量范围
    /// </summary>
    public int? MinFavoriteCount { get; private set; }
    public int? MaxFavoriteCount { get; private set; }

    /// <summary>
    /// 解析后的排序参数
    /// </summary>
    public string SortBy { get; private set; } = "CreatedAt";
    public string SortDirection { get; private set; } = "desc";
    public bool SortByRelevance { get; private set; }

    /// <summary>
    /// 解析后的高亮参数
    /// </summary>
    public bool EnableHighlight { get; private set; }
    public string HighlightPreTag { get; private set; } = "<em>";
    public string HighlightPostTag { get; private set; } = "</em>";

    /// <summary>
    /// 是否有搜索关键词（用于判断是否计算相关性）
    /// </summary>
    public bool HasSearchKeyword => !string.IsNullOrWhiteSpace(SearchKeyword);

    /// <summary>
    /// 是否需要按相关性排序
    /// </summary>
    public bool ShouldSortByRelevance => HasSearchKeyword && SortByRelevance;

    /// <summary>
    /// 从查询参数解析搜索条件
    /// </summary>
    /// <param name="parameters">原始查询参数</param>
    /// <returns>解析后的搜索条件</returns>
    public static MaterialSearchFilter Parse(MaterialQueryParameters parameters)
    {
        var filter = new MaterialSearchFilter();

        filter.ParsePagination(parameters);
        filter.ParseKeyword(parameters);
        filter.ParseCategories(parameters);
        filter.ParseTags(parameters);
        filter.ParseStatus(parameters);
        filter.ParseOwnerId(parameters);
        filter.ParseDateRange(parameters);
        filter.ParseViewCountRange(parameters);
        filter.ParseFavoriteCountRange(parameters);
        filter.ParseSorting(parameters);
        filter.ParseHighlightOptions(parameters);

        return filter;
    }

    private void ParsePagination(MaterialQueryParameters parameters)
    {
        PageNumber = Math.Max(1, parameters.PageNumber);
        PageSize = Math.Clamp(parameters.PageSize, 1, 100);
    }

    private void ParseKeyword(MaterialQueryParameters parameters)
    {
        SearchKeyword = parameters.SearchKeyword?.Trim();
        if (string.IsNullOrWhiteSpace(SearchKeyword))
        {
            SearchKeyword = null;
        }
    }

    private void ParseCategories(MaterialQueryParameters parameters)
    {
        var categories = new List<string>();

        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            categories.Add(parameters.Category.Trim());
        }

        if (!string.IsNullOrWhiteSpace(parameters.Categories))
        {
            var categoryList = parameters.Categories
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c));
            categories.AddRange(categoryList);
        }

        Categories = categories.Distinct().ToList();
    }

    private void ParseTags(MaterialQueryParameters parameters)
    {
        var tags = new List<string>();

        if (!string.IsNullOrWhiteSpace(parameters.Tag))
        {
            tags.Add(parameters.Tag.Trim());
        }

        if (!string.IsNullOrWhiteSpace(parameters.Tags))
        {
            var tagList = parameters.Tags
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t));
            tags.AddRange(tagList);
        }

        Tags = tags.Distinct().ToList();
    }

    private void ParseStatus(MaterialQueryParameters parameters)
    {
        Status = parameters.Status?.Trim();
        if (string.IsNullOrWhiteSpace(Status))
        {
            Status = null;
        }
    }

    private void ParseOwnerId(MaterialQueryParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.OwnerId) &&
            int.TryParse(parameters.OwnerId, out var ownerId))
        {
            OwnerId = ownerId;
        }
    }

    private void ParseDateRange(MaterialQueryParameters parameters)
    {
        StartDate = parameters.StartDate;
        EndDate = parameters.EndDate;

        if (EndDate.HasValue && !StartDate.HasValue)
        {
            StartDate = DateTime.MinValue;
        }

        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            (StartDate, EndDate) = (EndDate, StartDate);
        }
    }

    private void ParseViewCountRange(MaterialQueryParameters parameters)
    {
        MinViewCount = parameters.MinViewCount;
        MaxViewCount = parameters.MaxViewCount;

        if (MinViewCount < 0)
        {
            MinViewCount = 0;
        }

        if (MaxViewCount < 0)
        {
            MaxViewCount = null;
        }

        if (MinViewCount.HasValue && MaxViewCount.HasValue && MinViewCount > MaxViewCount)
        {
            (MinViewCount, MaxViewCount) = (MaxViewCount, MinViewCount);
        }
    }

    private void ParseFavoriteCountRange(MaterialQueryParameters parameters)
    {
        MinFavoriteCount = parameters.MinFavoriteCount;
        MaxFavoriteCount = parameters.MaxFavoriteCount;

        if (MinFavoriteCount < 0)
        {
            MinFavoriteCount = 0;
        }

        if (MaxFavoriteCount < 0)
        {
            MaxFavoriteCount = null;
        }

        if (MinFavoriteCount.HasValue && MaxFavoriteCount.HasValue && MinFavoriteCount > MaxFavoriteCount)
        {
            (MinFavoriteCount, MaxFavoriteCount) = (MaxFavoriteCount, MinFavoriteCount);
        }
    }

    private void ParseSorting(MaterialQueryParameters parameters)
    {
        SortByRelevance = parameters.SortByRelevance;

        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var validSortFields = new[]
            {
                "CreatedAt", "UpdatedAt", "ViewCount", "FavoriteCount",
                "Title", "Name", "Category", "Id"
            };

            if (validSortFields.Contains(parameters.SortBy, StringComparer.OrdinalIgnoreCase))
            {
                SortBy = parameters.SortBy;
            }
        }

        if (!string.IsNullOrWhiteSpace(parameters.SortDirection))
        {
            var dir = parameters.SortDirection.Trim().ToLower();
            SortDirection = dir == "asc" ? "asc" : "desc";
        }
    }

    private void ParseHighlightOptions(MaterialQueryParameters parameters)
    {
        EnableHighlight = parameters.EnableHighlight;

        if (!string.IsNullOrWhiteSpace(parameters.HighlightPreTag))
        {
            HighlightPreTag = parameters.HighlightPreTag;
        }

        if (!string.IsNullOrWhiteSpace(parameters.HighlightPostTag))
        {
            HighlightPostTag = parameters.HighlightPostTag;
        }
    }
}
