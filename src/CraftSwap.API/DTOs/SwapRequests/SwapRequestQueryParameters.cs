namespace CraftSwap.DTOs.SwapRequests;

/// <summary>
/// 交换请求查询参数
/// </summary>
public class SwapRequestQueryParameters
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
    /// 状态
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 请求者ID
    /// </summary>
    public string? RequesterId { get; set; }

    /// <summary>
    /// 被请求者ID
    /// </summary>
    public string? ResponderId { get; set; }

    /// <summary>
    /// 用户ID（作为请求者或被请求者）
    /// </summary>
    public string? UserId { get; set; }
}
