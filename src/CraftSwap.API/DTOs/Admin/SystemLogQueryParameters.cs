namespace CraftSwap.DTOs.Admin;

/// <summary>
/// 系统日志查询参数
/// </summary>
public class SystemLogQueryParameters
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
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 日志级别过滤
    /// </summary>
    public string? LogLevel { get; set; }

    /// <summary>
    /// 事件类型过滤
    /// </summary>
    public string? EventType { get; set; }

    /// <summary>
    /// 操作人用户ID
    /// </summary>
    public int? OperatorId { get; set; }

    /// <summary>
    /// 目标用户ID
    /// </summary>
    public int? TargetUserId { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? SearchKeyword { get; set; }

    /// <summary>
    /// 排序方向
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}
