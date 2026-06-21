namespace CraftSwap.DTOs.SwapReviews;

/// <summary>
/// 交换评价查询参数
/// </summary>
public class SwapReviewQueryParameters
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
    /// 被评价者ID
    /// </summary>
    public string? RevieweeId { get; set; }

    /// <summary>
    /// 评价者ID
    /// </summary>
    public string? ReviewerId { get; set; }

    /// <summary>
    /// 评分
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// 交换请求ID
    /// </summary>
    public int? SwapRequestId { get; set; }
}
