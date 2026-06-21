using CraftSwap.Entities;

namespace CraftSwap.Repositories;

/// <summary>
/// 用户仓储接口
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>用户对象</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// 根据邮箱获取用户
    /// </summary>
    /// <param name="email">邮箱</param>
    /// <returns>用户对象</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// 根据用户名或邮箱获取用户
    /// </summary>
    /// <param name="usernameOrEmail">用户名或邮箱</param>
    /// <returns>用户对象</returns>
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);

    /// <summary>
    /// 根据日期范围获取用户数量
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>用户数量</returns>
    Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate);
}
