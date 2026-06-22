namespace CraftSwap.DTOs.Stats;

/// <summary>
/// 趋势数据查询参数
/// </summary>
public class TrendQueryParameters
{
    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 时间粒度：日/周/月，默认按日
    /// </summary>
    public TrendTimeGranularity Granularity { get; set; } = TrendTimeGranularity.Day;
}
