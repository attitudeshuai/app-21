using CraftSwap.Entities;

namespace CraftSwap.Repositories;

/// <summary>
/// 交换评价仓储接口
/// </summary>
public interface ISwapReviewRepository : IRepository<SwapReview>
{
    /// <summary>
    /// 根据交换请求ID获取评价列表
    /// </summary>
    /// <param name="requestId">交换请求ID</param>
    /// <returns>评价列表</returns>
    Task<IEnumerable<SwapReview>> GetByRequestIdAsync(int requestId);

    /// <summary>
    /// 根据被评价人ID获取评价列表
    /// </summary>
    /// <param name="revieweeId">被评价人ID</param>
    /// <returns>评价列表</returns>
    Task<IEnumerable<SwapReview>> GetByRevieweeIdAsync(int revieweeId);

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
    Task<(IEnumerable<SwapReview> Items, int TotalCount)> GetPagedWithFiltersAsync(
        int pageNumber,
        int pageSize,
        int? requestId = null,
        int? reviewerId = null,
        int? revieweeId = null,
        int? rating = null,
        string? sortBy = null,
        string? sortDirection = null);

    /// <summary>
    /// 根据请求ID和评价人ID获取评价
    /// </summary>
    /// <param name="requestId">交换请求ID</param>
    /// <param name="reviewerId">评价人ID</param>
    /// <returns>评价对象</returns>
    Task<SwapReview?> GetByRequestIdAndReviewerIdAsync(int requestId, int reviewerId);
}
