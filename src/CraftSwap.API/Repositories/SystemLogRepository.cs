using CraftSwap.Common;
using CraftSwap.Data;
using CraftSwap.DTOs.Admin;
using CraftSwap.Entities;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Repositories;

/// <summary>
/// 系统日志仓储实现
/// </summary>
public class SystemLogRepository : Repository<SystemLog>, ISystemLogRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public SystemLogRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 分页查询系统日志
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页日志列表和总数</returns>
    public async Task<(List<SystemLog> Logs, int TotalCount)> GetPagedLogsAsync(SystemLogQueryParameters parameters)
    {
        var query = _dbSet.AsQueryable();

        if (parameters.StartTime.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= parameters.StartTime.Value);
        }

        if (parameters.EndTime.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= parameters.EndTime.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.LogLevel))
        {
            query = query.Where(l => l.LogLevel == parameters.LogLevel);
        }

        if (!string.IsNullOrWhiteSpace(parameters.EventType))
        {
            query = query.Where(l => l.EventType == parameters.EventType);
        }

        if (parameters.OperatorId.HasValue)
        {
            query = query.Where(l => l.OperatorId == parameters.OperatorId.Value);
        }

        if (parameters.TargetUserId.HasValue)
        {
            query = query.Where(l => l.TargetUserId == parameters.TargetUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchKeyword))
        {
            var keyword = parameters.SearchKeyword.ToLower();
            query = query.Where(l =>
                l.Message.ToLower().Contains(keyword) ||
                (l.Details != null && l.Details.ToLower().Contains(keyword)));
        }

        var totalCount = await query.CountAsync();

        var sortDesc = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        query = sortDesc
            ? query.OrderByDescending(l => l.CreatedAt)
            : query.OrderBy(l => l.CreatedAt);

        var pageNumber = Math.Max(1, parameters.PageNumber);
        var pageSize = Math.Min(Math.Max(1, parameters.PageSize), AppConstants.MaxPageSize);

        var logs = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalCount);
    }

    /// <summary>
    /// 创建系统日志
    /// </summary>
    /// <param name="log">日志实体</param>
    /// <returns>创建的日志实体</returns>
    public async Task<SystemLog> CreateLogAsync(SystemLog log)
    {
        log.CreatedAt = DateTime.UtcNow;
        return await AddAsync(log);
    }
}
