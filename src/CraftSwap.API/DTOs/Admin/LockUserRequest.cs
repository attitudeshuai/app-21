namespace CraftSwap.DTOs.Admin;

/// <summary>
/// 锁定用户请求
/// </summary>
public class LockUserRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 锁定时长（分钟），null表示永久锁定
    /// </summary>
    public int? LockDurationMinutes { get; set; }

    /// <summary>
    /// 锁定原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
