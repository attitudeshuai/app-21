using CraftSwap.Common;

namespace CraftSwap.Services;

/// <summary>
/// 系统日志服务接口
/// </summary>
public interface ISystemLogService
{
    /// <summary>
    /// 记录系统日志
    /// </summary>
    /// <param name="logLevel">日志级别</param>
    /// <param name="eventType">事件类型</param>
    /// <param name="message">日志消息</param>
    /// <param name="operatorId">操作人ID</param>
    /// <param name="operatorName">操作人名称</param>
    /// <param name="targetUserId">目标用户ID</param>
    /// <param name="details">详细信息</param>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="userAgent">用户代理</param>
    /// <returns>任务</returns>
    Task LogAsync(
        string logLevel,
        string eventType,
        string message,
        int? operatorId = null,
        string? operatorName = null,
        int? targetUserId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// 记录信息级别日志
    /// </summary>
    Task LogInformationAsync(
        string eventType,
        string message,
        int? operatorId = null,
        string? operatorName = null,
        int? targetUserId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// 记录警告级别日志
    /// </summary>
    Task LogWarningAsync(
        string eventType,
        string message,
        int? operatorId = null,
        string? operatorName = null,
        int? targetUserId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// 记录错误级别日志
    /// </summary>
    Task LogErrorAsync(
        string eventType,
        string message,
        int? operatorId = null,
        string? operatorName = null,
        int? targetUserId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null);
}
