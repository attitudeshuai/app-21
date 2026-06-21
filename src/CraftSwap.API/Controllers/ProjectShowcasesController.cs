using CraftSwap.DTOs.ProjectShowcases;
using CraftSwap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CraftSwap.Controllers;

/// <summary>
/// 作品展示控制器
/// </summary>
[ApiController]
[Route("api/project-showcases")]
public class ProjectShowcasesController : ControllerBase
{
    private readonly IProjectShowcaseService _projectShowcaseService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="projectShowcaseService">作品服务</param>
    public ProjectShowcasesController(IProjectShowcaseService projectShowcaseService)
    {
        _projectShowcaseService = projectShowcaseService;
    }

    /// <summary>
    /// 获取作品列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页作品响应</returns>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ProjectShowcaseQueryParameters parameters)
    {
        var response = await _projectShowcaseService.GetPagedAsync(parameters);
        return Ok(response);
    }

    /// <summary>
    /// 获取我发布的作品
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页作品响应</returns>
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] ProjectShowcaseQueryParameters parameters)
    {
        var response = await _projectShowcaseService.GetMyShowcasesAsync(parameters);
        return Ok(response);
    }

    /// <summary>
    /// 创建作品
    /// </summary>
    /// <param name="request">创建作品请求</param>
    /// <returns>作品响应</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProjectShowcaseRequest request)
    {
        var response = await _projectShowcaseService.CreateAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// 获取作品详情
    /// </summary>
    /// <param name="id">作品ID</param>
    /// <returns>作品响应</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _projectShowcaseService.GetByIdAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// 更新作品
    /// </summary>
    /// <param name="id">作品ID</param>
    /// <param name="request">更新作品请求</param>
    /// <returns>作品响应</returns>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectShowcaseRequest request)
    {
        var response = await _projectShowcaseService.UpdateAsync(id, request);
        return Ok(response);
    }

    /// <summary>
    /// 删除作品
    /// </summary>
    /// <param name="id">作品ID</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _projectShowcaseService.DeleteAsync(id);
        return Ok(response);
    }
}
