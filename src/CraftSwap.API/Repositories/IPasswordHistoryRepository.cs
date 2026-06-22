using CraftSwap.Entities;

namespace CraftSwap.Repositories;

/// <summary>
/// 密码历史仓储接口
/// </summary>
public interface IPasswordHistoryRepository : IRepository<PasswordHistory>
{
    /// <summary>
    /// 获取用户的最近N条密码历史记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">记录数量</param>
    /// <returns>密码历史列表</returns>
    Task<List<PasswordHistory>> GetRecentByUserIdAsync(int userId, int count);

    /// <summary>
    /// 获取用户的所有密码哈希
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>密码哈希列表</returns>
    Task<List<string>> GetPasswordHashesByUserIdAsync(int userId);

    /// <summary>
    /// 添加密码历史记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="passwordHash">密码哈希</param>
    /// <returns>创建的密码历史记录</returns>
    Task<PasswordHistory> AddAsync(int userId, string passwordHash);
}
