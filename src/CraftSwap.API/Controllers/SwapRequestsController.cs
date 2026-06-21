using CraftSwap.DTOs.SwapRequests;
using CraftSwap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CraftSwap.Controllers;

/// <summary>
/// 交换请求控制器
/// </summary>
[ApiController]
[Route("api/swap-requests")]
public class SwapRequestsController : ControllerBase
{
    private readonly ISwapRequestService _swapRequestService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="swapRequestService">交换请求服务</param>
    public SwapRequestsController(ISwapRequestService swapRequestService)
    {
        _swapRequestService = swapRequestService;
    }

    /// <summary>
    /// 获取交换请求列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页交换请求响应</returns>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SwapRequestQueryParameters parameters)
    {
        var response = await _swapRequestService.GetPagedAsync(parameters);
        return Ok(response);
    }

    /// <summary>
    /// 创建交换请求
    /// </summary>
    /// <param name="request">创建交换请求请求</param>
    /// <returns>交换请求响应</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateSwapRequestRequest request)
    {
        var response = await _swapRequestService.CreateAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// 获取交换请求详情
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <returns>交换请求响应</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _swapRequestService.GetByIdAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// 更新交换请求
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <param name="request">更新交换请求请求</param>
    /// <returns>交换请求响应</returns>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSwapRequestRequest request)
    {
        var response = await _swapRequestService.UpdateAsync(id, request);
        return Ok(response);
    }

    /// <summary>
    /// 删除交换请求
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _swapRequestService.DeleteAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// 修改交换请求状态
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <param name="request">更新状态请求</param>
    /// <returns>交换请求响应</returns>
    [HttpPatch("{id:int}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateSwapRequestStatusRequest request)
    {
        var response = await _swapRequestService.UpdateStatusAsync(id, request.Status);
        return Ok(response);
    }
}
