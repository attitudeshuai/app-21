namespace CraftSwap.Services;

/// <summary>
/// 令牌校验服务接口（凭证校验层）
/// </summary>
public interface ITokenValidatorService
{
    /// <summary>
    /// 校验访问令牌（JWT），即使令牌已过期也能提取Claims（仅用于刷新场景）
    /// </summary>
    /// <param name="accessToken">访问令牌</param>
    /// <param name="validateLifetime">是否校验生命周期（true=严格校验，false=允许过期令牌提取信息）</param>
    /// <returns>校验结果</returns>
    Task<TokenValidationResult> ValidateAccessTokenAsync(string accessToken, bool validateLifetime = true);

    /// <summary>
    /// 校验刷新令牌（含存储层校验：是否存在、是否过期、是否被吊销）
    /// </summary>
    /// <param name="refreshToken">刷新令牌值</param>
    /// <returns>校验结果</returns>
    Task<TokenValidationResult> ValidateRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// 校验访问令牌与刷新令牌是否匹配（同一对会话凭证）
    /// </summary>
    /// <param name="accessTokenJti">访问令牌的JTI</param>
    /// <param name="refreshToken">刷新令牌实体</param>
    /// <returns>是否匹配</returns>
    bool ValidateTokenPair(string accessTokenJti, Entities.RefreshToken refreshToken);
}
