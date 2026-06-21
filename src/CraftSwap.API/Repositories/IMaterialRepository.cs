using CraftSwap.Entities;

namespace CraftSwap.Repositories;

/// <summary>
/// 材料仓储接口
/// </summary>
public interface IMaterialRepository : IRepository<Material>
{
    /// <summary>
    /// 根据用户ID获取材料列表
    /// </summary>
    /// <param name="ownerId">用户ID</param>
    /// <returns>材料列表</returns>
    Task<IEnumerable<Material>> GetByOwnerIdAsync(int ownerId);

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
    Task<(IEnumerable<Material> Items, int TotalCount)> GetPagedWithFiltersAsync(
        int pageNumber,
        int pageSize,
        string? searchKeyword = null,
        string? category = null,
        string? color = null,
        string? status = null,
        int? ownerId = null,
        string? sortBy = null,
        string? sortDirection = null);

    /// <summary>
    /// 根据日期范围获取材料数量
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>材料数量</returns>
    Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate);
}
