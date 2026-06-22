namespace CraftSwap.DTOs.Stats;

/// <summary>
/// 趋势数据点
/// </summary>
public class TrendDataPoint
{
    /// <summary>
    /// 时间标签（日：yyyy-MM-dd，周：yyyy-MM-dd~yyyy-MM-dd，月：yyyy-MM）
    /// </summary>
    public string TimeLabel { get; set; } = string.Empty;

    /// <summary>
    /// 新增材料数量
    /// </summary>
    public int NewMaterials { get; set; }

    /// <summary>
    /// 新增交换请求数量
    /// </summary>
    public int NewSwapRequests { get; set; }

    /// <summary>
    /// 完成交换数量
    /// </summary>
    public int CompletedSwaps { get; set; }

    /// <summary>
    /// 用户注册数量
    /// </summary>
    public int NewUsers { get; set; }
}
