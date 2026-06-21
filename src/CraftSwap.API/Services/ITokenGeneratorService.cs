using CraftSwap.Entities;

namespace CraftSwap.Services;

/// <summary>
/// 令牌生成结果
/// </summary>
public class TokenGenerationResult
{
    /// <summary>
    /// 生成的JWT访问令牌
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 生成的刷新令牌值
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 访问令牌的JTI（唯一标识）
    /// </summary>
    public string AccessTokenJti { get; set; } = string.Empty;

    /// <summary>
    /// 访问令牌过期时长（秒）
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 访问令牌的绝对过期时间
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// 刷新令牌的绝对过期时间
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }
}

/// <summary>
/// 令牌生成服务接口（令牌生成层）
/// </summary>
public interface ITokenGeneratorService
{
    /// <summary>
    /// 为指定用户生成一组新的令牌（访问令牌+刷新令牌）
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns>令牌生成结果</returns>
    TokenGenerationResult GenerateTokenPair(User user);
}
