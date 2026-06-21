namespace CraftSwap.DTOs.Admin;

/// <summary>
/// 用户查询参数
/// </summary>
public class UserQueryParameters
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
    /// 搜索关键词（用户名或邮箱）
    /// </summary>
    public string? SearchKeyword { get; set; }

    /// <summary>
    /// 角色过滤
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// 是否只显示锁定用户
    /// </summary>
    public bool? IsLocked { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// 排序方向
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}
