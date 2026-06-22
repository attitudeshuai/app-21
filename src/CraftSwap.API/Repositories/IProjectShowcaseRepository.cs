using CraftSwap.Entities;

namespace CraftSwap.Repositories;

/// <summary>
/// 作品展示仓储接口
/// </summary>
public interface IProjectShowcaseRepository : IRepository<ProjectShowcase>
{
    /// <summary>
    /// 根据用户ID获取作品列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>作品列表</returns>
    Task<IEnumerable<ProjectShowcase>> GetByUserIdAsync(int userId);

    /// <summary>
    /// 分页获取作品（带筛选条件）
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="userId">用户ID</param>
    /// <param name="searchKeyword">搜索关键词</param>
    /// <param name="category">分类</param>
    /// <param name="tag">标签</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDirection">排序方向</param>
    /// <returns>分页后的作品列表和总数</returns>
    Task<(IEnumerable<ProjectShowcase> Items, int TotalCount)> GetPagedWithFiltersAsync(
        int pageNumber,
        int pageSize,
        int? userId = null,
        string? searchKeyword = null,
        string? category = null,
        string? tag = null,
        string? sortBy = null,
        string? sortDirection = null);

    /// <summary>
    /// 根据日期范围获取作品数量
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>作品数量</returns>
    Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate);
}
