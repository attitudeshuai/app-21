using CraftSwap.Common;
using CraftSwap.Entities;

namespace CraftSwap.Services;

public interface ISwapRequestStatusValidator
{
    bool IsValidStatus(string status);

    bool CanTransition(string currentStatus, string targetStatus);

    (bool Allowed, string ErrorMessage) ValidateTransition(SwapRequest swapRequest, string targetStatus);

    bool IsTerminalStatus(string status);
}

public class SwapRequestStatusValidator : ISwapRequestStatusValidator
{
    private static readonly Dictionary<string, HashSet<string>> _allowedTransitions = new()
    {
        [AppConstants.SwapRequestStatus.Pending] =
            [AppConstants.SwapRequestStatus.Accepted, AppConstants.SwapRequestStatus.Rejected, AppConstants.SwapRequestStatus.Cancelled],
        [AppConstants.SwapRequestStatus.Accepted] =
            [AppConstants.SwapRequestStatus.InProgress, AppConstants.SwapRequestStatus.Cancelled],
        [AppConstants.SwapRequestStatus.InProgress] =
            [AppConstants.SwapRequestStatus.Completed, AppConstants.SwapRequestStatus.Cancelled],
        [AppConstants.SwapRequestStatus.Rejected] = [],
        [AppConstants.SwapRequestStatus.Cancelled] = [],
        [AppConstants.SwapRequestStatus.Completed] = []
    };

    private static readonly HashSet<string> _allStatuses =
    [
        AppConstants.SwapRequestStatus.Pending,
        AppConstants.SwapRequestStatus.Accepted,
        AppConstants.SwapRequestStatus.Rejected,
        AppConstants.SwapRequestStatus.Cancelled,
        AppConstants.SwapRequestStatus.InProgress,
        AppConstants.SwapRequestStatus.Completed
    ];

    private static readonly HashSet<string> _terminalStatuses =
    [
        AppConstants.SwapRequestStatus.Rejected,
        AppConstants.SwapRequestStatus.Cancelled,
        AppConstants.SwapRequestStatus.Completed
    ];

    public bool IsValidStatus(string status)
    {
        return !string.IsNullOrWhiteSpace(status) && _allStatuses.Contains(status);
    }

    public bool IsTerminalStatus(string status)
    {
        return _terminalStatuses.Contains(status);
    }

    public bool CanTransition(string currentStatus, string targetStatus)
    {
        if (!_allowedTransitions.TryGetValue(currentStatus, out var allowedTargets))
        {
            return false;
        }

        return allowedTargets.Contains(targetStatus);
    }

    public (bool Allowed, string ErrorMessage) ValidateTransition(SwapRequest swapRequest, string targetStatus)
    {
        if (!IsValidStatus(targetStatus))
        {
            return (false, $"无效的交换请求状态: {targetStatus}");
        }

        var currentStatus = swapRequest.Status;

        if (currentStatus == targetStatus)
        {
            return (false, $"交换请求已处于 {GetStatusDisplayName(currentStatus)} 状态，无需重复操作");
        }

        if (IsTerminalStatus(currentStatus))
        {
            return (false, $"交换请求已 {GetStatusDisplayName(currentStatus)}，无法再变更状态");
        }

        if (!CanTransition(currentStatus, targetStatus))
        {
            return (false, $"不允许从 {GetStatusDisplayName(currentStatus)} 状态变更为 {GetStatusDisplayName(targetStatus)} 状态");
        }

        return (true, string.Empty);
    }

    private static string GetStatusDisplayName(string status)
    {
        return status switch
        {
            AppConstants.SwapRequestStatus.Pending => "待处理",
            AppConstants.SwapRequestStatus.Accepted => "已接受",
            AppConstants.SwapRequestStatus.Rejected => "已拒绝",
            AppConstants.SwapRequestStatus.Cancelled => "已取消",
            AppConstants.SwapRequestStatus.InProgress => "进行中",
            AppConstants.SwapRequestStatus.Completed => "已完成",
            _ => status
        };
    }
}
