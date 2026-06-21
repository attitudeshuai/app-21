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
    public async Task<IActionResult> GetOverview()
    {
        var response = await _statsService.GetOverviewAsync();
        return Ok(response);
    }

    /// <summary>
    /// 趋势统计
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>趋势统计响应</returns>
    [HttpGet("trend")]
    public async Task<IActionResult> GetTrend([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var response = await _statsService.GetTrendAsync(startDate, endDate);
        return Ok(response);
    }
}
