using CraftSwap.DTOs.Stats;
using CraftSwap.Filters;
using CraftSwap.Services;
using Microsoft.AspNetCore.Mvc;

namespace CraftSwap.Controllers;

/// <summary>
/// 统计控制器
/// </summary>
[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="statsService">统计服务</param>
    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    /// <summary>
    /// 总览统计
    /// </summary>
    /// <returns>概览统计响应</returns>
    [HttpGet("overview")]
    [RequireAdmin]
    public async Task<IActionResult> GetOverview()
    {
        var response = await _statsService.GetOverviewAsync();
        return Ok(response);
    }

    /// <summary>
    /// 趋势统计（旧版，按日粒度，向后兼容）
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>趋势统计响应</returns>
    [HttpGet("trend")]
    [RequireAdmin]
    public async Task<IActionResult> GetTrend([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var response = await _statsService.GetTrendAsync(startDate, endDate);
        return Ok(response);
    }

    /// <summary>
    /// 细粒度趋势统计
    /// </summary>
    /// <param name="startDate">开始日期（可选，默认7天前）</param>
    /// <param name="endDate">结束日期（可选，默认今天）</param>
    /// <param name="granularity">时间粒度：Day=按日，Week=按周，Month=按月（可选，默认按日）</param>
    /// <returns>细粒度趋势统计响应</returns>
    [HttpGet("trend/detail")]
    [RequireAdmin]
    public async Task<IActionResult> GetTrendDetail(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] TrendTimeGranularity granularity = TrendTimeGranularity.Day)
    {
        var parameters = new TrendQueryParameters
        {
            StartDate = startDate ?? default,
            EndDate = endDate ?? default,
            Granularity = granularity
        };

        var response = await _statsService.GetTrendDetailAsync(parameters);
        return Ok(response);
    }
}
