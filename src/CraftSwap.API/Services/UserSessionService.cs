using CraftSwap.Entities;
using CraftSwap.Repositories;

namespace CraftSwap.Services;

/// <summary>
/// 用户会话服务实现（会话存储层）
/// </summary>
public class UserSessionService : IUserSessionService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="refreshTokenRepository">刷新令牌仓储</param>
    public UserSessionService(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <summary>
    /// 创建新的用户会话（存储刷新令牌）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="refreshToken">刷新令牌值</param>
    /// <param name="accessTokenJti">关联的访问令牌JTI</param>
    /// <param name="refreshTokenExpiresAt">刷新令牌过期时间</param>
    /// <returns>创建的刷新令牌实体</returns>
    public async Task<RefreshToken> CreateSessionAsync(
        int userId,
        string refreshToken,
        string accessTokenJti,
        DateTime refreshTokenExpiresAt)
    {
        var tokenEntity = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            AccessTokenJti = accessTokenJti,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow
        };

        return await _refreshTokenRepository.AddAsync(tokenEntity);
    }

    /// <summary>
    /// 吊销指定的旧会话（使旧凭证立即失效）
    /// </summary>
    /// <param name="refreshTokenId">刷新令牌ID</param>
    /// <returns>任务</returns>
    public async Task RevokeSessionAsync(int refreshTokenId)
    {
        var tokenEntity = await _refreshTokenRepository.GetByIdAsync(refreshTokenId);
        if (tokenEntity != null && !tokenEntity.IsRevoked)
        {
            tokenEntity.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(tokenEntity);
        }
    }

    /// <summary>
    /// 根据访问令牌JTI吊销会话
    /// </summary>
    /// <param name="accessTokenJti">访问令牌JTI</param>
    /// <returns>是否成功吊销</returns>
    public async Task<bool> RevokeSessionByAccessTokenJtiAsync(string accessTokenJti)
    {
        var tokenEntity = await _refreshTokenRepository.GetByAccessTokenJtiAsync(accessTokenJti);
        if (tokenEntity != null && !tokenEntity.IsRevoked)
        {
            tokenEntity.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(tokenEntity);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 更新会话的最近使用时间
    /// </summary>
    /// <param name="refreshTokenId">刷新令牌ID</param>
    /// <returns>任务</returns>
    public async Task UpdateLastUsedAtAsync(int refreshTokenId)
    {
        var tokenEntity = await _refreshTokenRepository.GetByIdAsync(refreshTokenId);
        if (tokenEntity != null)
        {
            tokenEntity.LastUsedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(tokenEntity);
        }
    }

    /// <summary>
    /// 吊销用户的所有活跃会话
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>任务</returns>
    public async Task RevokeAllSessionsAsync(int userId)
    {
        await _refreshTokenRepository.RevokeAllByUserIdAsync(userId);
    }
}
