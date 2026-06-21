namespace CraftSwap.DTOs.Admin;

/// <summary>
/// 解锁用户请求
/// </summary>
public class UnlockUserRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 解锁原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
