using System.Security.Claims;

namespace CraftSwap.Services;

/// <summary>
/// 权限服务接口（权限判断层）
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 检查当前用户是否为管理员
    /// </summary>
    /// <param name="user">当前用户ClaimsPrincipal</param>
    /// <returns>是否为管理员</returns>
    bool IsAdmin(ClaimsPrincipal user);

    /// <summary>
    /// 获取当前用户ID
    /// </summary>
    /// <param name="user">当前用户ClaimsPrincipal</param>
    /// <returns>用户ID，未认证返回null</returns>
    int? GetCurrentUserId(ClaimsPrincipal user);

    /// <summary>
    /// 获取当前用户名
    /// </summary>
    /// <param name="user">当前用户ClaimsPrincipal</param>
    /// <returns>用户名，未认证返回null</returns>
    string? GetCurrentUserName(ClaimsPrincipal user);

    /// <summary>
    /// 获取当前用户角色
    /// </summary>
    /// <param name="user">当前用户ClaimsPrincipal</param>
    /// <returns>用户角色，未认证返回null</returns>
    string? GetCurrentUserRole(ClaimsPrincipal user);

    /// <summary>
    /// 确保当前用户是管理员，否则抛出异常
    /// </summary>
    /// <param name="user">当前用户ClaimsPrincipal</param>
    void EnsureAdmin(ClaimsPrincipal user);
}
