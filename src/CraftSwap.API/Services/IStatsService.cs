using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.Stats;

namespace CraftSwap.Services;

/// <summary>
/// 统计服务接口
/// </summary>
public interface IStatsService
{
    /// <summary>
    /// 获取概览统计数据
    /// </summary>
    /// <returns>概览统计响应</returns>
    Task<ApiResponse<OverviewStatsResponse>> GetOverviewAsync();

    /// <summary>
    /// 获取趋势统计数据（旧版，按日粒度，向后兼容）
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>趋势统计响应</returns>
    Task<ApiResponse<TrendStatsResponse>> GetTrendAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 获取细粒度趋势统计数据
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>细粒度趋势统计响应</returns>
    Task<ApiResponse<TrendStatsDetailResponse>> GetTrendDetailAsync(TrendQueryParameters parameters);
}
