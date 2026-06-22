using System.Security.Claims;
using CraftSwap.Common;
using CraftSwap.Entities;
using CraftSwap.Exceptions;

namespace CraftSwap.Services;

public class PermissionService : IPermissionService
{
    private static readonly Dictionary<string, string> _operationRequiredRole = new()
    {
        [AppConstants.SwapRequestStatus.Accepted] = "Receiver",
        [AppConstants.SwapRequestStatus.Rejected] = "Receiver",
        [AppConstants.SwapRequestStatus.Cancelled] = "Proposer",
        [AppConstants.SwapRequestStatus.InProgress] = "Either",
        [AppConstants.SwapRequestStatus.Completed] = "Either"
    };

    private static readonly Dictionary<string, string> _operationDisplayName = new()
    {
        [AppConstants.SwapRequestStatus.Accepted] = "接受",
        [AppConstants.SwapRequestStatus.Rejected] = "拒绝",
        [AppConstants.SwapRequestStatus.Cancelled] = "取消",
        [AppConstants.SwapRequestStatus.InProgress] = "开始进行",
        [AppConstants.SwapRequestStatus.Completed] = "完成"
    };

    public bool IsAdmin(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var role = GetCurrentUserRole(user);
        return role == AppConstants.UserRoles.Admin;
    }

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

    public string? GetCurrentUserName(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirst(ClaimTypes.Name)?.Value;
    }

    public string? GetCurrentUserRole(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    public void EnsureAdmin(ClaimsPrincipal user)
    {
        if (!IsAdmin(user))
        {
            throw new BusinessException(403, "权限不足，需要管理员角色");
        }
    }

    public (bool Allowed, string ErrorMessage) ValidateSwapRequestOperation(int userId, SwapRequest swapRequest, string targetStatus)
    {
        var isProposer = swapRequest.ProposerId == userId;
        var isReceiver = swapRequest.ReceiverId == userId;

        if (!isProposer && !isReceiver)
        {
            return (false, "您不是该交换请求的参与方，无权操作");
        }

        if (!_operationRequiredRole.TryGetValue(targetStatus, out var requiredRole))
        {
            return (false, $"不支持的操作类型: {_operationDisplayName.GetValueOrDefault(targetStatus, targetStatus)}");
        }

        var operationName = _operationDisplayName.GetValueOrDefault(targetStatus, targetStatus);

        return requiredRole switch
        {
            "Receiver" when !isReceiver => (false, $"仅接收方可以{operationName}交换请求"),
            "Proposer" when !isProposer => (false, $"仅发起方可以{operationName}交换请求"),
            "Either" when !isProposer && !isReceiver => (false, $"仅交换请求的参与方可以{operationName}交换请求"),
            _ => (true, string.Empty)
        };
    }
}
