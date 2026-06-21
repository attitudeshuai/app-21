namespace CraftSwap.DTOs.Auth;

/// <summary>
/// 认证响应
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public UserResponse User { get; set; } = new UserResponse();

    /// <summary>
    /// 访问令牌
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 令牌过期时间（秒）
    /// </summary>
    public int ExpiresIn { get; set; }
}
