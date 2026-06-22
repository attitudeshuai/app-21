namespace CraftSwap.DTOs.Stats;

/// <summary>
/// 细粒度趋势统计响应
/// </summary>
public class TrendStatsDetailResponse
{
    /// <summary>
    /// 时间粒度
    /// </summary>
    public TrendTimeGranularity Granularity { get; set; }

    /// <summary>
    /// 实际开始日期
    /// </summary>
    public DateTime ActualStartDate { get; set; }

    /// <summary>
    /// 实际结束日期
    /// </summary>
    public DateTime ActualEndDate { get; set; }

    /// <summary>
    /// 数据点总数
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// 趋势数据点列表
    /// </summary>
    public List<TrendDataPoint> DataPoints { get; set; } = new List<TrendDataPoint>();

    /// <summary>
    /// 汇总统计
    /// </summary>
    public TrendSummary Summary { get; set; } = new TrendSummary();
}

/// <summary>
/// 趋势汇总统计
/// </summary>
public class TrendSummary
{
    /// <summary>
    /// 时间段内新增材料总数
    /// </summary>
    public int TotalNewMaterials { get; set; }

    /// <summary>
    /// 时间段内新增交换请求总数
    /// </summary>
    public int TotalNewSwapRequests { get; set; }

    /// <summary>
    /// 时间段内完成交换总数
    /// </summary>
    public int TotalCompletedSwaps { get; set; }

    /// <summary>
    /// 时间段内新增用户总数
    /// </summary>
    public int TotalNewUsers { get; set; }

    /// <summary>
    /// 日均新增材料数
    /// </summary>
    public double AvgNewMaterialsPerDay { get; set; }

    /// <summary>
    /// 日均新增交换请求数
    /// </summary>
    public double AvgNewSwapRequestsPerDay { get; set; }

    /// <summary>
    /// 日均完成交换数
    /// </summary>
    public double AvgCompletedSwapsPerDay { get; set; }

    /// <summary>
    /// 日均新增用户数
    /// </summary>
    public double AvgNewUsersPerDay { get; set; }
}
