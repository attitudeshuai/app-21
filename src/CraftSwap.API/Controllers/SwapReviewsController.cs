using CraftSwap.DTOs.SwapReviews;
using CraftSwap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CraftSwap.Controllers;

/// <summary>
/// 评价控制器
/// </summary>
[ApiController]
[Route("api/swap-reviews")]
public class SwapReviewsController : ControllerBase
{
    private readonly ISwapReviewService _swapReviewService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="swapReviewService">评价服务</param>
    public SwapReviewsController(ISwapReviewService swapReviewService)
    {
        _swapReviewService = swapReviewService;
    }

    /// <summary>
    /// 获取评价列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页评价响应</returns>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SwapReviewQueryParameters parameters)
    {
        var response = await _swapReviewService.GetPagedAsync(parameters);
        return Ok(response);
    }

    /// <summary>
    /// 创建评价
    /// </summary>
    /// <param name="request">创建评价请求</param>
    /// <returns>评价响应</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateSwapReviewRequest request)
    {
        var response = await _swapReviewService.CreateAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// 获取评价详情
    /// </summary>
    /// <param name="id">评价ID</param>
    /// <returns>评价响应</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _swapReviewService.GetByIdAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// 更新评价
    /// </summary>
    /// <param name="id">评价ID</param>
    /// <param name="request">更新评价请求</param>
    /// <returns>评价响应</returns>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSwapReviewRequest request)
    {
        var response = await _swapReviewService.UpdateAsync(id, request);
        return Ok(response);
    }

    /// <summary>
    /// 删除评价
    /// </summary>
    /// <param name="id">评价ID</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _swapReviewService.DeleteAsync(id);
        return Ok(response);
    }
}
