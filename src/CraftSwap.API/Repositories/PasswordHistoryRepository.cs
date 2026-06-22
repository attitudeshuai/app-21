using CraftSwap.Data;
using CraftSwap.Entities;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Repositories;

/// <summary>
/// 密码历史仓储实现
/// </summary>
public class PasswordHistoryRepository : Repository<PasswordHistory>, IPasswordHistoryRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public PasswordHistoryRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 获取用户的最近N条密码历史记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">记录数量</param>
    /// <returns>密码历史列表</returns>
    public async Task<List<PasswordHistory>> GetRecentByUserIdAsync(int userId, int count)
    {
        return await _dbSet
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// 获取用户的所有密码哈希
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>密码哈希列表</returns>
    public async Task<List<string>> GetPasswordHashesByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .Select(ph => ph.PasswordHash)
            .ToListAsync();
    }

    /// <summary>
    /// 添加密码历史记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="passwordHash">密码哈希</param>
    /// <returns>创建的密码历史记录</returns>
    public async Task<PasswordHistory> AddAsync(int userId, string passwordHash)
    {
        var history = new PasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _dbSet.Add(history);
        await _context.SaveChangesAsync();

        return history;
    }
}
