using CraftSwap.Data;
using CraftSwap.Entities;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Repositories;

/// <summary>
/// 交换评价仓储实现
/// </summary>
public class SwapReviewRepository : Repository<SwapReview>, ISwapReviewRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public SwapReviewRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 根据交换请求ID获取评价列表
    /// </summary>
    /// <param name="requestId">交换请求ID</param>
    /// <returns>评价列表</returns>
    public async Task<IEnumerable<SwapReview>> GetByRequestIdAsync(int requestId)
    {
        return await _dbSet
            .Where(r => r.RequestId == requestId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 根据被评价人ID获取评价列表
    /// </summary>
    /// <param name="revieweeId">被评价人ID</param>
    /// <returns>评价列表</returns>
    public async Task<IEnumerable<SwapReview>> GetByRevieweeIdAsync(int revieweeId)
    {
        return await _dbSet
            .Where(r => r.RevieweeId == revieweeId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 分页获取评价（带筛选条件）
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="requestId">交换请求ID</param>
    /// <param name="reviewerId">评价人ID</param>
    /// <param name="revieweeId">被评价人ID</param>
    /// <param name="rating">评分</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDirection">排序方向</param>
    /// <returns>分页后的评价列表和总数</returns>
    public async Task<(IEnumerable<SwapReview> Items, int TotalCount)> GetPagedWithFiltersAsync(
        int pageNumber,
        int pageSize,
        int? requestId = null,
        int? reviewerId = null,
        int? revieweeId = null,
        int? rating = null,
        string? sortBy = null,
        string? sortDirection = null)
    {
        var query = _dbSet.AsQueryable();

        if (requestId.HasValue)
        {
            query = query.Where(r => r.RequestId == requestId.Value);
        }

        if (reviewerId.HasValue)
        {
            query = query.Where(r => r.ReviewerId == reviewerId.Value);
        }

        if (revieweeId.HasValue)
        {
            query = query.Where(r => r.RevieweeId == revieweeId.Value);
        }

        if (rating.HasValue)
        {
            query = query.Where(r => r.Rating == rating.Value);
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
    /// 根据请求ID和评价人ID获取评价
    /// </summary>
    /// <param name="requestId">交换请求ID</param>
    /// <param name="reviewerId">评价人ID</param>
    /// <returns>评价对象</returns>
    public async Task<SwapReview?> GetByRequestIdAndReviewerIdAsync(int requestId, int reviewerId)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.RequestId == requestId && r.ReviewerId == reviewerId);
    }
}
