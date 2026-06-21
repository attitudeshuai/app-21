using CraftSwap.Entities;
using CraftSwap.Repositories;

namespace CraftSwap.Services;

/// <summary>
/// 系统日志服务实现
/// </summary>
public class SystemLogService : ISystemLogService
{
    private readonly ISystemLogRepository _systemLogRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="systemLogRepository">系统日志仓储</param>
    public SystemLogService(ISystemLogRepository systemLogRepository)
    {
        _systemLogRepository = systemLogRepository;
    }

    /// <summary>
    /// 记录系统日志
    /// </summary>
    public async Task LogAsync(
        string logLevel,
        string eventType,
        string message,
        int? operatorId = null,
        string? operatorName = null,
        int? targetUserId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var log = new SystemLog
        {
            LogLevel = logLevel,
            EventType = eventType,
            Message = message,
            OperatorId = operatorId,
            OperatorName = operatorName,
            TargetUserId = targetUserId,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        await _systemLogRepository.CreateLogAsync(log);
    }

    /// <summary>
    /// 记录信息级别日志
    /// </summary>
    public async Task LogInformationAsync(
        string eventType,
        string message,
        int? operatorId = null,
        string? operatorName = null,
        int? targetUserId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        await LogAsync(
            Common.AppConstants.LogLevels.Information,
            eventType,
            message,
            operatorId,
            operatorName,
            targetUserId,
            details,
            ipAddress,
            userAgent);
    }

    /// <summary>
    /// 记录警告级别日志
    /// </summary>
    public async Task LogWarningAsync(
        string eventType,
        string message,
        int? operatorId = null,
        string? operatorName = null,
        int? targetUserId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        await LogAsync(
            Common.AppConstants.LogLevels.Warning,
            eventType,
            message,
            operatorId,
            operatorName,
            targetUserId,
            details,
            ipAddress,
            userAgent);
    }

    /// <summary>
    /// 记录错误级别日志
    /// </summary>
    public async Task LogErrorAsync(
        string eventType,
        string message,
        int? operatorId = null,
        string? operatorName = null,
        int? targetUserId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        await LogAsync(
            Common.AppConstants.LogLevels.Error,
            eventType,
            message,
            operatorId,
            operatorName,
            targetUserId,
            details,
            ipAddress,
            userAgent);
    }
}
