using CraftSwap.Data;
using CraftSwap.Entities;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Repositories;

/// <summary>
/// 材料仓储实现
/// </summary>
public class MaterialRepository : Repository<Material>, IMaterialRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public MaterialRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 根据用户ID获取材料列表
    /// </summary>
    /// <param name="ownerId">用户ID</param>
    /// <returns>材料列表</returns>
    public async Task<IEnumerable<Material>> GetByOwnerIdAsync(int ownerId)
    {
        return await _dbSet
            .Where(m => m.OwnerId == ownerId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 分页获取材料（带筛选条件）
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="searchKeyword">搜索关键词</param>
    /// <param name="category">分类</param>
    /// <param name="color">颜色</param>
    /// <param name="status">状态</param>
    /// <param name="ownerId">所有者ID</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDirection">排序方向</param>
    /// <returns>分页后的材料列表和总数</returns>
    public async Task<(IEnumerable<Material> Items, int TotalCount)> GetPagedWithFiltersAsync(
        int pageNumber,
        int pageSize,
        string? searchKeyword = null,
        string? category = null,
        string? color = null,
        string? status = null,
        int? ownerId = null,
        string? sortBy = null,
        string? sortDirection = null)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(m =>
                m.Name.Contains(searchKeyword) ||
                m.Condition.Contains(searchKeyword) ||
                m.Category.Contains(searchKeyword) ||
                m.MaterialType.Contains(searchKeyword) ||
                m.Color.Contains(searchKeyword));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(m => m.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(color))
        {
            query = query.Where(m => m.Color == color);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status == status);
        }

        if (ownerId.HasValue)
        {
            query = query.Where(m => m.OwnerId == ownerId.Value);
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
    /// 根据日期范围获取材料数量
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>材料数量</returns>
    public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.CountAsync(m => m.CreatedAt >= startDate && m.CreatedAt < endDate);
    }
}
