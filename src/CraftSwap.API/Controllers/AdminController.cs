using CraftSwap.DTOs.Admin;
using CraftSwap.Filters;
using CraftSwap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CraftSwap.Controllers;

/// <summary>
/// 管理员控制器
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize]
[RequireAdmin]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IPermissionService _permissionService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="adminService">管理员服务</param>
    /// <param name="permissionService">权限服务</param>
    public AdminController(
        IAdminService adminService,
        IPermissionService permissionService)
    {
        _adminService = adminService;
        _permissionService = permissionService;
    }

    /// <summary>
    /// 分页查询用户列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页用户列表</returns>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryParameters parameters)
    {
        var operatorId = _permissionService.GetCurrentUserId(User)!.Value;
        var operatorName = _permissionService.GetCurrentUserName(User)!;

        var response = await _adminService.GetPagedUsersAsync(parameters, operatorId, operatorName);
        return Ok(response);
    }

    /// <summary>
    /// 锁定用户账户
    /// </summary>
    /// <param name="request">锁定请求</param>
    /// <returns>锁定结果</returns>
    [HttpPost("users/lock")]
    public async Task<IActionResult> LockUser([FromBody] LockUserRequest request)
    {
        var operatorId = _permissionService.GetCurrentUserId(User)!.Value;
        var operatorName = _permissionService.GetCurrentUserName(User)!;

        var response = await _adminService.LockUserAsync(request, operatorId, operatorName);
        return Ok(response);
    }

    /// <summary>
    /// 解锁用户账户
    /// </summary>
    /// <param name="request">解锁请求</param>
    /// <returns>解锁结果</returns>
    [HttpPost("users/unlock")]
    public async Task<IActionResult> UnlockUser([FromBody] UnlockUserRequest request)
    {
        var operatorId = _permissionService.GetCurrentUserId(User)!.Value;
        var operatorName = _permissionService.GetCurrentUserName(User)!;

        var response = await _adminService.UnlockUserAsync(request, operatorId, operatorName);
        return Ok(response);
    }

    /// <summary>
    /// 按时间范围分页查询系统日志
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页日志列表</returns>
    [HttpGet("logs")]
    public async Task<IActionResult> GetSystemLogs([FromQuery] SystemLogQueryParameters parameters)
    {
        var operatorId = _permissionService.GetCurrentUserId(User)!.Value;
        var operatorName = _permissionService.GetCurrentUserName(User)!;

        var response = await _adminService.GetPagedSystemLogsAsync(parameters, operatorId, operatorName);
        return Ok(response);
    }
}
