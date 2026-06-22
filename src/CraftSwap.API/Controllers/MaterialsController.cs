using CraftSwap.DTOs.Materials;
using CraftSwap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CraftSwap.Controllers;

/// <summary>
/// 材料控制器
/// </summary>
[ApiController]
[Route("api/materials")]
public class MaterialsController : ControllerBase
{
    private readonly IMaterialService _materialService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="materialService">材料服务</param>
    public MaterialsController(IMaterialService materialService)
    {
        _materialService = materialService;
    }

    /// <summary>
    /// 获取材料列表（分页、搜索、筛选）- 向后兼容
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页材料响应</returns>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] MaterialQueryParameters parameters)
    {
        var response = await _materialService.GetPagedAsync(parameters);
        return Ok(response);
    }

    /// <summary>
    /// 高级搜索材料（支持多条件组合过滤、相关性排序、关键词高亮）
    /// 支持筛选条件：材料类别、标签、发布时间区间、浏览热度范围、收藏数量范围
    /// 支持功能：分页、排序、关键词高亮、相关性加权
    /// </summary>
    /// <param name="parameters">高级查询参数</param>
    /// <returns>分页材料响应（含高亮和相关性分数）</returns>
    [HttpGet("search")]
    public async Task<IActionResult> AdvancedSearch([FromQuery] MaterialQueryParameters parameters)
    {
        var response = await _materialService.AdvancedSearchAsync(parameters);
        return Ok(response);
    }

    /// <summary>
    /// 获取我发布的材料
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页材料响应</returns>
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] MaterialQueryParameters parameters)
    {
        var response = await _materialService.GetMyMaterialsAsync(parameters);
        return Ok(response);
    }

    /// <summary>
    /// 创建材料
    /// </summary>
    /// <param name="request">创建材料请求</param>
    /// <returns>材料响应</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateMaterialRequest request)
    {
        var response = await _materialService.CreateAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// 获取材料详情
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <returns>材料响应</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _materialService.GetByIdAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// 更新材料
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <param name="request">更新材料请求</param>
    /// <returns>材料响应</returns>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMaterialRequest request)
    {
        var response = await _materialService.UpdateAsync(id, request);
        return Ok(response);
    }

    /// <summary>
    /// 删除材料
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _materialService.DeleteAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// 修改材料状态
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <param name="request">更新状态请求</param>
    /// <returns>材料响应</returns>
    [HttpPatch("{id:int}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateMaterialStatusRequest request)
    {
        var response = await _materialService.UpdateStatusAsync(id, request.Status);
        return Ok(response);
    }
}
