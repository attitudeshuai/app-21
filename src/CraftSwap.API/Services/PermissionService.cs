using System.Security.Claims;
using CraftSwap.Common;
using CraftSwap.Exceptions;

namespace CraftSwap.Services;

/// <summary>
/// 权限服务实现（权限判断层）
/// </summary>
public class PermissionService : IPermissionService
{
    /// <summary>
    /// 检查当前用户是否为管理员
    /// </summary>
    /// <param name="user">当前用户ClaimsPrincipal</param>
    /// <returns>是否为管理员</returns>
    public bool IsAdmin(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var role = GetCurrentUserRole(user);
        return role == AppConstants.UserRoles.Admin;
    }

    /// <summary>
    /// 获取当前用户ID
    /// </summary>
    /// <param name="user">当前用户ClaimsPrincipal</param>
    /// <returns>用户ID，未认证返回null</returns>
    public int? GetCurrentUserId(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim?.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// 获取当前用户名
    /// </summary>
    /// <param name="user">当前用户ClaimsPrincipal</param>
    /// <returns>用户名，未认证返回null</returns>
    public string? GetCurrentUserName(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// 获取当前用户角色
    /// </summary>
    /// <param name="user">当前用户ClaimsPrincipal</param>
    /// <returns>用户角色，未认证返回null</returns>
    public string? GetCurrentUserRole(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// 确保当前用户是管理员，否则抛出异常
    /// </summary>
    /// <param name="user">当前用户ClaimsPrincipal</param>
    public void EnsureAdmin(ClaimsPrincipal user)
    {
        if (!IsAdmin(user))
        {
            throw new BusinessException(403, "权限不足，需要管理员角色");
        }
    }
}
