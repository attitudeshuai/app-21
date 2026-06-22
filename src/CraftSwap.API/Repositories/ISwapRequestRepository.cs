using CraftSwap.Entities;

namespace CraftSwap.Repositories;

/// <summary>
/// 交换请求仓储接口
/// </summary>
public interface ISwapRequestRepository : IRepository<SwapRequest>
{
    /// <summary>
    /// 根据发起方用户ID获取交换请求列表
    /// </summary>
    /// <param name="proposerId">发起方用户ID</param>
    /// <returns>交换请求列表</returns>
    Task<IEnumerable<SwapRequest>> GetByProposerIdAsync(int proposerId);

    /// <summary>
    /// 根据接收方用户ID获取交换请求列表
    /// </summary>
    /// <param name="receiverId">接收方用户ID</param>
    /// <returns>交换请求列表</returns>
    Task<IEnumerable<SwapRequest>> GetByReceiverIdAsync(int receiverId);

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
    Task<(IEnumerable<SwapRequest> Items, int TotalCount)> GetPagedWithFiltersAsync(
        int pageNumber,
        int pageSize,
        int? proposerId = null,
        int? receiverId = null,
        string? status = null,
        string? sortBy = null,
        string? sortDirection = null);

    /// <summary>
    /// 根据日期范围获取交换请求数量
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>交换请求数量</returns>
    Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 根据状态获取交换请求数量
    /// </summary>
    /// <param name="status">状态</param>
    /// <returns>交换请求数量</returns>
    Task<int> GetCountByStatusAsync(string status);

    /// <summary>
    /// 根据状态和日期范围获取交换请求数量
    /// </summary>
    /// <param name="status">状态</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>交换请求数量</returns>
    Task<int> GetCountByStatusAndDateRangeAsync(string status, DateTime startDate, DateTime endDate);

    /// <summary>
    /// 根据日期范围按日获取新增交换请求数量分组
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>日期和数量的字典</returns>
    Task<Dictionary<DateTime, int>> GetDailyCountGroupedAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 根据日期范围按日获取指定状态交换请求数量分组
    /// </summary>
    /// <param name="status">状态</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>日期和数量的字典</returns>
    Task<Dictionary<DateTime, int>> GetDailyCountByStatusGroupedAsync(string status, DateTime startDate, DateTime endDate);
}
