using CraftSwap.Common;
using CraftSwap.DTOs.Common;
using CraftSwap.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CraftSwap.Filters;

/// <summary>
/// 管理员权限过滤器
/// </summary>
public class RequireAdminAttribute : TypeFilterAttribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public RequireAdminAttribute() : base(typeof(RequireAdminFilter))
    {
    }

    /// <summary>
    /// 权限过滤器实现
    /// </summary>
    private class RequireAdminFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionService _permissionService;
        private readonly ISystemLogService _systemLogService;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RequireAdminFilter(
            IPermissionService permissionService,
            ISystemLogService systemLogService)
        {
            _permissionService = permissionService;
            _systemLogService = systemLogService;
        }

        /// <summary>
        /// 权限验证
        /// </summary>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new ObjectResult(ApiResponse.Fail("未授权访问", 401))
                {
                    StatusCode = 401
                };
                return;
            }

            if (!_permissionService.IsAdmin(user))
            {
                var operatorId = _permissionService.GetCurrentUserId(user);
                var operatorName = _permissionService.GetCurrentUserName(user);
                var ipAddress = GetClientIpAddress(context.HttpContext);
                var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

                await _systemLogService.LogWarningAsync(
                    AppConstants.EventTypes.PermissionDenied,
                    $"用户 {operatorName} 尝试访问管理员接口被拒绝",
                    operatorId,
                    operatorName,
                    ipAddress: ipAddress,
                    userAgent: userAgent);

                context.Result = new ObjectResult(ApiResponse.Fail("权限不足，需要管理员角色", 403))
                {
                    StatusCode = 403
                };
                return;
            }
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        private static string? GetClientIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                return forwardedFor.Split(',').FirstOrDefault()?.Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
