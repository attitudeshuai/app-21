using CraftSwap.Data;
using CraftSwap.Entities;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Repositories;

/// <summary>
/// 作品展示仓储实现
/// </summary>
public class ProjectShowcaseRepository : Repository<ProjectShowcase>, IProjectShowcaseRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public ProjectShowcaseRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 根据用户ID获取作品列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>作品列表</returns>
    public async Task<IEnumerable<ProjectShowcase>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 分页获取作品（带筛选条件）
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="userId">用户ID</param>
    /// <param name="searchKeyword">搜索关键词</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDirection">排序方向</param>
    /// <returns>分页后的作品列表和总数</returns>
    public async Task<(IEnumerable<ProjectShowcase> Items, int TotalCount)> GetPagedWithFiltersAsync(
        int pageNumber,
        int pageSize,
        int? userId = null,
        string? searchKeyword = null,
        string? sortBy = null,
        string? sortDirection = null)
    {
        var query = _dbSet.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(p =>
                p.Title.Contains(searchKeyword) ||
                p.Description.Contains(searchKeyword));
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
    /// 根据日期范围获取作品数量
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>作品数量</returns>
    public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.CountAsync(p => p.CreatedAt >= startDate && p.CreatedAt < endDate);
    }
}
