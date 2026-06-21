namespace CraftSwap.DTOs.Common;

/// <summary>
/// 分页响应
/// </summary>
/// <typeparam name="T">数据项类型</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 数据项列表
    /// </summary>
    public List<T> Items { get; set; } = new List<T>();
}
