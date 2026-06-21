using CraftSwap.DTOs.Admin;
using CraftSwap.DTOs.Common;

namespace CraftSwap.Services;

/// <summary>
/// 管理员服务接口（业务逻辑层）
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// 分页查询用户列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <param name="operatorId">操作人ID</param>
    /// <param name="operatorName">操作人名称</param>
    /// <returns>分页用户列表</returns>
    Task<PagedResponse<UserAdminResponse>> GetPagedUsersAsync(
        UserQueryParameters parameters,
        int operatorId,
        string operatorName);

    /// <summary>
    /// 锁定用户账户
    /// </summary>
    /// <param name="request">锁定请求</param>
    /// <param name="operatorId">操作人ID</param>
    /// <param name="operatorName">操作人名称</param>
    /// <returns>锁定结果</returns>
    Task<UserLockResponse> LockUserAsync(
        LockUserRequest request,
        int operatorId,
        string operatorName);

    /// <summary>
    /// 解锁用户账户
    /// </summary>
    /// <param name="request">解锁请求</param>
    /// <param name="operatorId">操作人ID</param>
    /// <param name="operatorName">操作人名称</param>
    /// <returns>解锁结果</returns>
    Task<UserLockResponse> UnlockUserAsync(
        UnlockUserRequest request,
        int operatorId,
        string operatorName);

    /// <summary>
    /// 按时间范围分页查询系统日志
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <param name="operatorId">操作人ID</param>
    /// <param name="operatorName">操作人名称</param>
    /// <returns>分页日志列表</returns>
    Task<PagedResponse<SystemLogResponse>> GetPagedSystemLogsAsync(
        SystemLogQueryParameters parameters,
        int operatorId,
        string operatorName);
}
