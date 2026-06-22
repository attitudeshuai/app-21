using System.Security.Claims;
using CraftSwap.Common;
using CraftSwap.Entities;

namespace CraftSwap.Services;

public interface IPermissionService
{
    bool IsAdmin(ClaimsPrincipal user);

    int? GetCurrentUserId(ClaimsPrincipal user);

    string? GetCurrentUserName(ClaimsPrincipal user);

    string? GetCurrentUserRole(ClaimsPrincipal user);

    void EnsureAdmin(ClaimsPrincipal user);

    (bool Allowed, string ErrorMessage) ValidateSwapRequestOperation(int userId, SwapRequest swapRequest, string targetStatus);
}
