namespace CraftSwap.DTOs.Admin;

/// <summary>
/// 用户锁定操作响应
/// </summary>
public class UserLockResponse
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 是否已锁定
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// 锁定结束时间
    /// </summary>
    public DateTime? LockEndTime { get; set; }

    /// <summary>
    /// 锁定原因
    /// </summary>
    public string? LockReason { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    public DateTime OperatedAt { get; set; }
}
