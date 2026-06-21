using System.Text.Json;
using CraftSwap.Common;
using CraftSwap.DTOs.Admin;
using CraftSwap.DTOs.Common;
using CraftSwap.Entities;
using CraftSwap.Exceptions;
using CraftSwap.Repositories;

namespace CraftSwap.Services;

/// <summary>
/// 管理员服务实现（业务逻辑层）
/// </summary>
public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly ISystemLogRepository _systemLogRepository;
    private readonly ISystemLogService _systemLogService;
    private readonly IUserSessionService _userSessionService;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AdminService(
        IUserRepository userRepository,
        ISystemLogRepository systemLogRepository,
        ISystemLogService systemLogService,
        IUserSessionService userSessionService)
    {
        _userRepository = userRepository;
        _systemLogRepository = systemLogRepository;
        _systemLogService = systemLogService;
        _userSessionService = userSessionService;
    }

    /// <summary>
    /// 分页查询用户列表
    /// </summary>
    public async Task<PagedResponse<UserAdminResponse>> GetPagedUsersAsync(
        UserQueryParameters parameters,
        int operatorId,
        string operatorName)
    {
        var (users, totalCount) = await _userRepository.GetPagedUsersAsync(parameters);

        var userResponses = users.Select(u => new UserAdminResponse
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Avatar = u.Avatar,
            Role = u.Role,
            IsLocked = u.IsLocked && (!u.LockEndTime.HasValue || u.LockEndTime.Value > DateTime.UtcNow),
            LockEndTime = u.LockEndTime,
            LockReason = u.LockReason,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        }).ToList();

        await _systemLogService.LogInformationAsync(
            AppConstants.EventTypes.UserListQueried,
            $"管理员 {operatorName} 查询了用户列表",
            operatorId,
            operatorName,
            details: JsonSerializer.Serialize(new { parameters.PageNumber, parameters.PageSize, parameters.SearchKeyword, parameters.Role, parameters.IsLocked }));

        return new PagedResponse<UserAdminResponse>
        {
            Items = userResponses,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
        };
    }

    /// <summary>
    /// 锁定用户账户
    /// </summary>
    public async Task<UserLockResponse> LockUserAsync(
        LockUserRequest request,
        int operatorId,
        string operatorName)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new BusinessException(404, "用户不存在");
        }

        if (user.Role == AppConstants.UserRoles.Admin)
        {
            throw new BusinessException(400, "不能锁定管理员账户");
        }

        if (user.Id == operatorId)
        {
            throw new BusinessException(400, "不能锁定自己的账户");
        }

        user.IsLocked = true;
        user.LockEndTime = request.LockDurationMinutes.HasValue
            ? DateTime.UtcNow.AddMinutes(request.LockDurationMinutes.Value)
            : null;
        user.LockReason = request.Reason;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _userSessionService.RevokeAllSessionsAsync(user.Id);

        await _systemLogService.LogWarningAsync(
            AppConstants.EventTypes.UserLocked,
            $"管理员 {operatorName} 锁定了用户 {user.Username} 的账户，原因：{request.Reason}",
            operatorId,
            operatorName,
            user.Id,
            JsonSerializer.Serialize(new { request.LockDurationMinutes, request.Reason }));

        return new UserLockResponse
        {
            UserId = user.Id,
            Username = user.Username,
            IsLocked = true,
            LockEndTime = user.LockEndTime,
            LockReason = user.LockReason,
            OperatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 解锁用户账户
    /// </summary>
    public async Task<UserLockResponse> UnlockUserAsync(
        UnlockUserRequest request,
        int operatorId,
        string operatorName)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new BusinessException(404, "用户不存在");
        }

        if (!user.IsLocked)
        {
            throw new BusinessException(400, "用户账户未被锁定");
        }

        user.IsLocked = false;
        user.LockEndTime = null;
        user.LockReason = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _systemLogService.LogInformationAsync(
            AppConstants.EventTypes.UserUnlocked,
            $"管理员 {operatorName} 解锁了用户 {user.Username} 的账户，原因：{request.Reason}",
            operatorId,
            operatorName,
            user.Id,
            JsonSerializer.Serialize(new { request.Reason }));

        return new UserLockResponse
        {
            UserId = user.Id,
            Username = user.Username,
            IsLocked = false,
            LockEndTime = null,
            LockReason = null,
            OperatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 按时间范围分页查询系统日志
    /// </summary>
    public async Task<PagedResponse<SystemLogResponse>> GetPagedSystemLogsAsync(
        SystemLogQueryParameters parameters,
        int operatorId,
        string operatorName)
    {
        var (logs, totalCount) = await _systemLogRepository.GetPagedLogsAsync(parameters);

        var logResponses = logs.Select(l => new SystemLogResponse
        {
            Id = l.Id,
            LogLevel = l.LogLevel,
            EventType = l.EventType,
            OperatorId = l.OperatorId,
            OperatorName = l.OperatorName,
            TargetUserId = l.TargetUserId,
            Message = l.Message,
            Details = l.Details,
            IpAddress = l.IpAddress,
            CreatedAt = l.CreatedAt
        }).ToList();

        await _systemLogService.LogInformationAsync(
            AppConstants.EventTypes.SystemLogQueried,
            $"管理员 {operatorName} 查询了系统日志",
            operatorId,
            operatorName,
            details: JsonSerializer.Serialize(new { parameters.PageNumber, parameters.PageSize, parameters.StartTime, parameters.EndTime, parameters.LogLevel, parameters.EventType }));

        return new PagedResponse<SystemLogResponse>
        {
            Items = logResponses,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
        };
    }
}
