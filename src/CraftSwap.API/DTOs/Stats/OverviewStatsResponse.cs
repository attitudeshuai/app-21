namespace CraftSwap.DTOs.Stats;

/// <summary>
/// 概览统计响应
/// </summary>
public class OverviewStatsResponse
{
    /// <summary>
    /// 用户总数
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// 材料总数
    /// </summary>
    public int TotalMaterials { get; set; }

    /// <summary>
    /// 交换请求总数
    /// </summary>
    public int TotalSwapRequests { get; set; }

    /// <summary>
    /// 已完成交换数
    /// </summary>
    public int CompletedSwaps { get; set; }

    /// <summary>
    /// 项目展示总数
    /// </summary>
    public int TotalProjectShowcases { get; set; }

    /// <summary>
    /// 评价总数
    /// </summary>
    public int TotalReviews { get; set; }

    /// <summary>
    /// 今日新增用户
    /// </summary>
    public int TodayNewUsers { get; set; }

    /// <summary>
    /// 今日新增材料
    /// </summary>
    public int TodayNewMaterials { get; set; }

    /// <summary>
    /// 今日新增交换请求
    /// </summary>
    public int TodayNewSwapRequests { get; set; }
}
