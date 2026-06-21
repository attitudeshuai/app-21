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
    /// 分类
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public string? OwnerId { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string? Tag { get; set; }
}
