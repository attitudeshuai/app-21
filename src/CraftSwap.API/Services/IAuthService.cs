using CraftSwap.DTOs.Auth;
using CraftSwap.DTOs.Common;

namespace CraftSwap.Services;

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="request">注册请求</param>
    /// <returns>认证响应</returns>
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录请求</param>
    /// <returns>认证响应</returns>
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns>用户响应</returns>
    Task<ApiResponse<UserResponse>> GetCurrentUserAsync();

    /// <summary>
    /// 更新用户个人资料
    /// </summary>
    /// <param name="request">更新资料请求</param>
    /// <returns>用户响应</returns>
    Task<ApiResponse<UserResponse>> UpdateProfileAsync(UpdateProfileRequest request);
}
