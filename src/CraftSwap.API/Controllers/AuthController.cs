using CraftSwap.DTOs.Auth;
using CraftSwap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CraftSwap.Controllers;

/// <summary>
/// 认证控制器
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="authService">认证服务</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="request">注册请求</param>
    /// <returns>认证响应</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录请求</param>
    /// <returns>认证响应</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns>用户响应</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var response = await _authService.GetCurrentUserAsync();
        return Ok(response);
    }

    /// <summary>
    /// 更新个人信息
    /// </summary>
    /// <param name="request">更新资料请求</param>
    /// <returns>用户响应</returns>
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var response = await _authService.UpdateProfileAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="request">修改密码请求</param>
    /// <returns>操作结果</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var response = await _authService.ChangePasswordAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// 刷新会话凭证
    /// </summary>
    /// <remarks>
    /// 当访问凭证即将过期或已过期时，可使用原凭证换取新的有效凭证组。
    /// 旧凭证会立即失效，同时更新最近使用时间。
    /// </remarks>
    /// <param name="request">刷新令牌请求（访问令牌 + 刷新令牌）</param>
    /// <returns>新的认证响应（包含新的访问令牌、刷新令牌、过期时间和用户信息）</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        return Ok(response);
    }
}
