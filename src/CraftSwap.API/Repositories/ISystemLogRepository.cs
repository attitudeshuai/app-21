using CraftSwap.DTOs.Admin;
using CraftSwap.Entities;

namespace CraftSwap.Repositories;

/// <summary>
/// 系统日志仓储接口
/// </summary>
public interface ISystemLogRepository : IRepository<SystemLog>
{
    /// <summary>
    /// 分页查询系统日志
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页日志列表和总数</returns>
    Task<(List<SystemLog> Logs, int TotalCount)> GetPagedLogsAsync(SystemLogQueryParameters parameters);

    /// <summary>
    /// 创建系统日志
    /// </summary>
    /// <param name="log">日志实体</param>
    /// <returns>创建的日志实体</returns>
    Task<SystemLog> CreateLogAsync(SystemLog log);
}
