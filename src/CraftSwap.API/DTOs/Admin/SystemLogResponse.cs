namespace CraftSwap.DTOs.Admin;

/// <summary>
/// 系统日志响应
/// </summary>
public class SystemLogResponse
{
    /// <summary>
    /// 日志ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    public string LogLevel { get; set; } = string.Empty;

    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// 操作人用户ID
    /// </summary>
    public int? OperatorId { get; set; }

    /// <summary>
    /// 操作人用户名
    /// </summary>
    public string? OperatorName { get; set; }

    /// <summary>
    /// 目标用户ID
    /// </summary>
    public int? TargetUserId { get; set; }

    /// <summary>
    /// 日志消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 详细信息
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
