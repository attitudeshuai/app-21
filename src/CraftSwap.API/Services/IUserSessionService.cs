using CraftSwap.Entities;

namespace CraftSwap.Services;

/// <summary>
/// 用户会话服务接口（会话存储层）
/// </summary>
public interface IUserSessionService
{
    /// <summary>
    /// 创建新的用户会话（存储刷新令牌）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="refreshToken">刷新令牌值</param>
    /// <param name="accessTokenJti">关联的访问令牌JTI</param>
    /// <param name="refreshTokenExpiresAt">刷新令牌过期时间</param>
    /// <returns>创建的刷新令牌实体</returns>
    Task<RefreshToken> CreateSessionAsync(
        int userId,
        string refreshToken,
        string accessTokenJti,
        DateTime refreshTokenExpiresAt);

    /// <summary>
    /// 吊销指定的旧会话（使旧凭证立即失效）
    /// </summary>
    /// <param name="refreshTokenId">刷新令牌ID</param>
    /// <returns>任务</returns>
    Task RevokeSessionAsync(int refreshTokenId);

    /// <summary>
    /// 根据访问令牌JTI吊销会话
    /// </summary>
    /// <param name="accessTokenJti">访问令牌JTI</param>
    /// <returns>是否成功吊销</returns>
    Task<bool> RevokeSessionByAccessTokenJtiAsync(string accessTokenJti);

    /// <summary>
    /// 更新会话的最近使用时间
    /// </summary>
    /// <param name="refreshTokenId">刷新令牌ID</param>
    /// <returns>任务</returns>
    Task UpdateLastUsedAtAsync(int refreshTokenId);

    /// <summary>
    /// 吊销用户的所有活跃会话
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>任务</returns>
    Task RevokeAllSessionsAsync(int userId);
}
