namespace CraftSwap.DTOs.Auth;

/// <summary>
/// 刷新令牌请求
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// 当前持有的访问令牌（可以是即将过期或已过期的）
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
