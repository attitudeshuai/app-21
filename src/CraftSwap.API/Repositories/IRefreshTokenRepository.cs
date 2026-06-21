using CraftSwap.Entities;

namespace CraftSwap.Repositories;

/// <summary>
/// 刷新令牌仓储接口
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// 根据刷新令牌值获取令牌实体（包含用户信息）
    /// </summary>
    /// <param name="token">刷新令牌值</param>
    /// <returns>刷新令牌实体</returns>
    Task<RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// 根据访问令牌JTI获取刷新令牌
    /// </summary>
    /// <param name="accessTokenJti">访问令牌JTI</param>
    /// <returns>刷新令牌实体</returns>
    Task<RefreshToken?> GetByAccessTokenJtiAsync(string accessTokenJti);

    /// <summary>
    /// 获取用户的所有有效刷新令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>刷新令牌列表</returns>
    Task<List<RefreshToken>> GetActiveByUserIdAsync(int userId);

    /// <summary>
    /// 吊销用户的所有刷新令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>任务</returns>
    Task RevokeAllByUserIdAsync(int userId);
}
