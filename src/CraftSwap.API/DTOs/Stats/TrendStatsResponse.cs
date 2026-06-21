namespace CraftSwap.DTOs.Stats;

/// <summary>
/// 趋势统计响应
/// </summary>
public class TrendStatsResponse
{
    /// <summary>
    /// 日期列表
    /// </summary>
    public List<string> Dates { get; set; } = new List<string>();

    /// <summary>
    /// 用户增长数据
    /// </summary>
    public List<int> UserGrowth { get; set; } = new List<int>();

    /// <summary>
    /// 材料增长数据
    /// </summary>
    public List<int> MaterialGrowth { get; set; } = new List<int>();

    /// <summary>
    /// 交换请求增长数据
    /// </summary>
    public List<int> SwapRequestGrowth { get; set; } = new List<int>();

    /// <summary>
    /// 完成交换增长数据
    /// </summary>
    public List<int> CompletedSwapGrowth { get; set; } = new List<int>();
}
