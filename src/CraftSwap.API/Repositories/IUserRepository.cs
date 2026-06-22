using CraftSwap.DTOs.Admin;
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

    /// <summary>
    /// 根据日期范围按日获取用户注册数量分组
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>日期和数量的字典</returns>
    Task<Dictionary<DateTime, int>> GetDailyCountGroupedAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 分页查询用户列表（管理员用）
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页用户列表和总数</returns>
    Task<(List<User> Users, int TotalCount)> GetPagedUsersAsync(UserQueryParameters parameters);

    /// <summary>
    /// 检查用户是否为管理员
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否为管理员</returns>
    Task<bool> IsUserAdminAsync(int userId);

    /// <summary>
    /// 检查账户是否被锁定
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否被锁定</returns>
    Task<bool> IsUserLockedAsync(int userId);

    Task UpdateReviewStatsAsync(int userId, decimal averageRating, int totalReviewCount);
}
