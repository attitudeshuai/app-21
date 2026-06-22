using CraftSwap.Entities;
using CraftSwap.Services;

namespace CraftSwap.Repositories;

/// <summary>
/// 材料搜索结果（包含相关性分数）
/// </summary>
public class MaterialSearchResult
{
    /// <summary>
    /// 材料实体
    /// </summary>
    public Material Material { get; set; } = null!;

    /// <summary>
    /// 相关性分数（仅在关键词搜索时有值）
    /// </summary>
    public double? RelevanceScore { get; set; }
}

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
    Task<(IEnumerable<Material> Items, int TotalCount)> GetPagedWithFiltersAsync(
        int pageNumber,
        int pageSize,
        string? searchKeyword = null,
        string? category = null,
        string? color = null,
        string? status = null,
        int? ownerId = null,
        string? sortBy = null,
        string? sortDirection = null,
        string? tag = null);

    /// <summary>
    /// 高级分页搜索材料（带所有筛选条件和相关性计算）
    /// </summary>
    /// <param name="filter">搜索条件过滤器</param>
    /// <returns>分页后的材料搜索结果列表和总数</returns>
    Task<(IEnumerable<MaterialSearchResult> Items, int TotalCount)> AdvancedSearchAsync(MaterialSearchFilter filter);

    /// <summary>
    /// 增加材料浏览数
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <returns>是否成功</returns>
    Task<bool> IncrementViewCountAsync(int id);

    /// <summary>
    /// 根据日期范围获取材料数量
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>材料数量</returns>
    Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 根据日期范围按日获取新增材料数量分组
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>日期和数量的字典</returns>
    Task<Dictionary<DateTime, int>> GetDailyCountGroupedAsync(DateTime startDate, DateTime endDate);
}
