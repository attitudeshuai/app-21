using CraftSwap.Data;
using CraftSwap.Entities;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Repositories;

/// <summary>
/// 交换请求仓储实现
/// </summary>
public class SwapRequestRepository : Repository<SwapRequest>, ISwapRequestRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public SwapRequestRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 根据发起方用户ID获取交换请求列表
    /// </summary>
    /// <param name="proposerId">发起方用户ID</param>
    /// <returns>交换请求列表</returns>
    public async Task<IEnumerable<SwapRequest>> GetByProposerIdAsync(int proposerId)
    {
        return await _dbSet
            .Where(s => s.ProposerId == proposerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 根据接收方用户ID获取交换请求列表
    /// </summary>
    /// <param name="receiverId">接收方用户ID</param>
    /// <returns>交换请求列表</returns>
    public async Task<IEnumerable<SwapRequest>> GetByReceiverIdAsync(int receiverId)
    {
        return await _dbSet
            .Where(s => s.ReceiverId == receiverId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 分页获取交换请求（带筛选条件）
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="proposerId">发起方用户ID</param>
    /// <param name="receiverId">接收方用户ID</param>
    /// <param name="status">状态</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDirection">排序方向</param>
    /// <returns>分页后的交换请求列表和总数</returns>
    public async Task<(IEnumerable<SwapRequest> Items, int TotalCount)> GetPagedWithFiltersAsync(
        int pageNumber,
        int pageSize,
        int? proposerId = null,
        int? receiverId = null,
        string? status = null,
        string? sortBy = null,
        string? sortDirection = null)
    {
        var query = _dbSet.AsQueryable();

        if (proposerId.HasValue)
        {
            query = query.Where(s => s.ProposerId == proposerId.Value);
        }

        if (receiverId.HasValue)
        {
            query = query.Where(s => s.ReceiverId == receiverId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => s.Status == status);
        }

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, sortBy ?? "CreatedAt", sortDirection ?? "desc");

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// 根据日期范围获取交换请求数量
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>交换请求数量</returns>
    public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.CountAsync(s => s.CreatedAt >= startDate && s.CreatedAt < endDate);
    }

    /// <summary>
    /// 根据状态获取交换请求数量
    /// </summary>
    /// <param name="status">状态</param>
    /// <returns>交换请求数量</returns>
    public async Task<int> GetCountByStatusAsync(string status)
    {
        return await _dbSet.CountAsync(s => s.Status == status);
    }

    /// <summary>
    /// 根据状态和日期范围获取交换请求数量
    /// </summary>
    /// <param name="status">状态</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>交换请求数量</returns>
    public async Task<int> GetCountByStatusAndDateRangeAsync(string status, DateTime startDate, DateTime endDate)
    {
        return await _dbSet.CountAsync(s => s.Status == status && s.CreatedAt >= startDate && s.CreatedAt < endDate);
    }

    /// <summary>
    /// 根据日期范围按日获取新增交换请求数量分组
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>日期和数量的字典</returns>
    public async Task<Dictionary<DateTime, int>> GetDailyCountGroupedAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt < endDate)
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);
    }

    /// <summary>
    /// 根据日期范围按日获取指定状态交换请求数量分组
    /// </summary>
    /// <param name="status">状态</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>日期和数量的字典</returns>
    public async Task<Dictionary<DateTime, int>> GetDailyCountByStatusGroupedAsync(string status, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(s => s.Status == status && s.CreatedAt >= startDate && s.CreatedAt < endDate)
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);
    }
}
