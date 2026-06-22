namespace CraftSwap.DTOs.Materials;

/// <summary>
/// 材料查询参数
/// </summary>
public class MaterialQueryParameters
{
    /// <summary>
    /// 页码
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? SearchKeyword { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 排序方向（asc/desc）
    /// </summary>
    public string? SortDirection { get; set; }

    /// <summary>
    /// 分类（单分类筛选，向后兼容）
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 分类列表（多分类筛选，逗号分隔）
    /// </summary>
    public string? Categories { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public string? OwnerId { get; set; }

    /// <summary>
    /// 标签（单标签筛选，向后兼容）
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 标签列表（多标签筛选，逗号分隔）
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 发布时间开始（含）
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 发布时间结束（含）
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 最小浏览量
    /// </summary>
    public int? MinViewCount { get; set; }

    /// <summary>
    /// 最大浏览量
    /// </summary>
    public int? MaxViewCount { get; set; }

    /// <summary>
    /// 最小收藏量
    /// </summary>
    public int? MinFavoriteCount { get; set; }

    /// <summary>
    /// 最大收藏量
    /// </summary>
    public int? MaxFavoriteCount { get; set; }

    /// <summary>有关键词时是否按相关性排序
    /// </summary>
    public bool SortByRelevance { get; set; } = true;

    /// <summary>
    /// 是否启用关键词高亮
    /// </summary>
    public bool EnableHighlight { get; set; } = true;

    /// <summary>
    /// 高亮前缀标签
    /// </summary>
    public string HighlightPreTag { get; set; } = "<em>";

    /// <summary>
    /// 高亮后缀标签
    /// </summary>
    public string HighlightPostTag { get; set; } = "</em>";
}
