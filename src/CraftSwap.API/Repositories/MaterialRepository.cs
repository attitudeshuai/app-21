using CraftSwap.Data;
using CraftSwap.Entities;
using CraftSwap.Services;
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
    /// 分页获取材料（带筛选条件）- 向后兼容
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
    /// <param name="tag">标签筛选</param>
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
        string? sortDirection = null,
        string? tag = null)
    {
        var query = _dbSet.AsQueryable();

        query = ApplyBasicFilters(query, searchKeyword, category, color, status, ownerId, tag);

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, sortBy ?? "CreatedAt", sortDirection ?? "desc");

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// 高级分页搜索材料（带所有筛选条件和相关性计算）
    /// </summary>
    /// <param name="filter">搜索条件过滤器</param>
    /// <returns>分页后的材料搜索结果列表和总数</returns>
    public async Task<(IEnumerable<MaterialSearchResult> Items, int TotalCount)> AdvancedSearchAsync(MaterialSearchFilter filter)
    {
        var query = _dbSet.AsQueryable();

        query = ApplyAdvancedFilters(query, filter);

        var totalCount = await query.CountAsync();

        if (filter.ShouldSortByRelevance)
        {
            var materials = await query.ToListAsync();
            var scoredResults = materials
                .Select(m => new MaterialSearchResult
                {
                    Material = m,
                    RelevanceScore = CalculateRelevanceScore(m, filter.SearchKeyword!)
                })
                .OrderByDescending(r => r.RelevanceScore)
                .ThenByDescending(r => r.Material.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return (scoredResults, totalCount);
        }
        else
        {
            query = ApplySorting(query, filter.SortBy, filter.SortDirection);

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var results = items.Select(m => new MaterialSearchResult
            {
                Material = m,
                RelevanceScore = null
            }).ToList();

            return (results, totalCount);
        }
    }

    /// <summary>
    /// 应用基础筛选条件（向后兼容）
    /// </summary>
    private static IQueryable<Material> ApplyBasicFilters(
        IQueryable<Material> query,
        string? searchKeyword,
        string? category,
        string? color,
        string? status,
        int? ownerId,
        string? tag)
    {
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(m =>
                (m.Title != null && m.Title.Contains(searchKeyword)) ||
                (m.Description != null && m.Description.Contains(searchKeyword)) ||
                m.Name.Contains(searchKeyword) ||
                m.Condition.Contains(searchKeyword) ||
                m.Category.Contains(searchKeyword) ||
                m.MaterialType.Contains(searchKeyword) ||
                m.Color.Contains(searchKeyword) ||
                (m.Tags != null && m.Tags.Contains(searchKeyword)));
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

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(m => m.Tags != null && m.Tags.Contains(tag));
        }

        return query;
    }

    /// <summary>
    /// 应用高级筛选条件
    /// </summary>
    private static IQueryable<Material> ApplyAdvancedFilters(
        IQueryable<Material> query,
        MaterialSearchFilter filter)
    {
        if (filter.HasSearchKeyword)
        {
            var keyword = filter.SearchKeyword!;
            query = query.Where(m =>
                (m.Title != null && m.Title.Contains(keyword)) ||
                (m.Description != null && m.Description.Contains(keyword)) ||
                m.Name.Contains(keyword) ||
                m.Condition.Contains(keyword) ||
                m.Category.Contains(keyword) ||
                m.MaterialType.Contains(keyword) ||
                m.Color.Contains(keyword) ||
                (m.Tags != null && m.Tags.Contains(keyword)));
        }

        if (filter.Categories.Count > 0)
        {
            query = query.Where(m => filter.Categories.Contains(m.Category));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(m => m.Status == filter.Status);
        }

        if (filter.OwnerId.HasValue)
        {
            query = query.Where(m => m.OwnerId == filter.OwnerId.Value);
        }

        if (filter.Tags.Count > 0)
        {
            foreach (var tag in filter.Tags)
            {
                var tagToMatch = tag;
                query = query.Where(m => m.Tags != null && m.Tags.Contains(tagToMatch));
            }
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(m => m.CreatedAt >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            var endDate = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(m => m.CreatedAt <= endDate);
        }

        if (filter.MinViewCount.HasValue)
        {
            query = query.Where(m => m.ViewCount >= filter.MinViewCount.Value);
        }

        if (filter.MaxViewCount.HasValue)
        {
            query = query.Where(m => m.ViewCount <= filter.MaxViewCount.Value);
        }

        if (filter.MinFavoriteCount.HasValue)
        {
            query = query.Where(m => m.FavoriteCount >= filter.MinFavoriteCount.Value);
        }

        if (filter.MaxFavoriteCount.HasValue)
        {
            query = query.Where(m => m.FavoriteCount <= filter.MaxFavoriteCount.Value);
        }

        return query;
    }

    /// <summary>
    /// 计算材料与搜索关键词的相关性分数
    /// 加权规则：
    /// - 标题完全匹配：100分
    /// - 标题包含：50分
    /// - 标签包含：30分
    /// - 分类匹配：20分
    /// - 描述包含：15分
    /// - 其他字段包含：10分
    /// </summary>
    /// <param name="material">材料实体</param>
    /// <param name="keyword">搜索关键词</param>
    /// <returns>相关性分数（0-100+）</returns>
    private static double CalculateRelevanceScore(Material material, string keyword)
    {
        double score = 0;
        var keywordLower = keyword.ToLowerInvariant();

        var title = (!string.IsNullOrEmpty(material.Title) ? material.Title : material.Name).ToLowerInvariant();
        if (title == keywordLower)
        {
            score += 100;
        }
        else if (title.Contains(keywordLower))
        {
            score += 50;
        }

        if (!string.IsNullOrEmpty(material.Tags))
        {
            var tags = material.Tags.ToLowerInvariant();
            if (tags.Contains(keywordLower))
            {
                score += 30;
                var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (tagList.Any(t => t.Trim() == keywordLower))
                {
                    score += 20;
                }
            }
        }

        if (material.Category.ToLowerInvariant() == keywordLower)
        {
            score += 20;
        }
        else if (material.Category.ToLowerInvariant().Contains(keywordLower))
        {
            score += 10;
        }

        var description = (!string.IsNullOrEmpty(material.Description) ? material.Description : material.Condition).ToLowerInvariant();
        if (description.Contains(keywordLower))
        {
            score += 15;
        }

        if (material.MaterialType.ToLowerInvariant().Contains(keywordLower))
        {
            score += 10;
        }

        if (material.Color.ToLowerInvariant().Contains(keywordLower))
        {
            score += 10;
        }

        score += Math.Log10(material.ViewCount + 1) * 2;
        score += Math.Log10(material.FavoriteCount + 1) * 3;

        var recencyBonus = Math.Max(0, 30 - (DateTime.UtcNow - material.CreatedAt).TotalDays / 10);
        score += Math.Max(0, recencyBonus);

        return score;
    }

    /// <summary>
    /// 增加材料浏览数
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <returns>是否成功</returns>
    public async Task<bool> IncrementViewCountAsync(int id)
    {
        var material = await _dbSet.FindAsync(id);
        if (material == null)
        {
            return false;
        }

        material.ViewCount += 1;
        material.UpdatedAt = DateTime.UtcNow;
        _context.Entry(material).Property(m => m.ViewCount).IsModified = true;
        _context.Entry(material).Property(m => m.UpdatedAt).IsModified = true;
        await _context.SaveChangesAsync();
        return true;
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
