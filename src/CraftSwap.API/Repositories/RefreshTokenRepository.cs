using CraftSwap.Data;
using CraftSwap.Entities;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Repositories;

/// <summary>
/// 刷新令牌仓储实现
/// </summary>
public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 根据刷新令牌值获取令牌实体（包含用户信息）
    /// </summary>
    /// <param name="token">刷新令牌值</param>
    /// <returns>刷新令牌实体</returns>
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    /// <summary>
    /// 根据访问令牌JTI获取刷新令牌
    /// </summary>
    /// <param name="accessTokenJti">访问令牌JTI</param>
    /// <returns>刷新令牌实体</returns>
    public async Task<RefreshToken?> GetByAccessTokenJtiAsync(string accessTokenJti)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.AccessTokenJti == accessTokenJti);
    }

    /// <summary>
    /// 获取用户的所有有效刷新令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>刷新令牌列表</returns>
    public async Task<List<RefreshToken>> GetActiveByUserIdAsync(int userId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(rt => rt.UserId == userId
                && rt.RevokedAt == null
                && rt.ExpiresAt > now)
            .ToListAsync();
    }

    /// <summary>
    /// 吊销用户的所有刷新令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>任务</returns>
    public async Task RevokeAllByUserIdAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var tokens = await _dbSet
            .Where(rt => rt.UserId == userId
                && rt.RevokedAt == null
                && rt.ExpiresAt > now)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = now;
        }

        if (tokens.Count > 0)
        {
            _dbSet.UpdateRange(tokens);
            await _context.SaveChangesAsync();
        }
    }
}
