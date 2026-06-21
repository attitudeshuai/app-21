using CraftSwap.Common;
using CraftSwap.Data;
using CraftSwap.DTOs.Admin;
using CraftSwap.Entities;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Repositories;

/// <summary>
/// 用户仓储实现
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>用户对象</returns>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>
    /// 根据邮箱获取用户
    /// </summary>
    /// <param name="email">邮箱</param>
    /// <returns>用户对象</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// 根据用户名或邮箱获取用户
    /// </summary>
    /// <param name="usernameOrEmail">用户名或邮箱</param>
    /// <returns>用户对象</returns>
    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _dbSet.FirstOrDefaultAsync(u =>
            u.Username == usernameOrEmail || u.Email == usernameOrEmail);
    }

    /// <summary>
    /// 根据日期范围获取用户数量
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>用户数量</returns>
    public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt < endDate);
    }

    /// <summary>
    /// 分页查询用户列表（管理员用）
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页用户列表和总数</returns>
    public async Task<(List<User> Users, int TotalCount)> GetPagedUsersAsync(UserQueryParameters parameters)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.SearchKeyword))
        {
            var keyword = parameters.SearchKeyword.ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(keyword) ||
                u.Email.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Role))
        {
            query = query.Where(u => u.Role == parameters.Role);
        }

        if (parameters.IsLocked.HasValue)
        {
            query = query.Where(u => u.IsLocked == parameters.IsLocked.Value);
        }

        var totalCount = await query.CountAsync();

        var sortBy = parameters.SortBy;
        var sortDesc = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy.ToLower() switch
        {
            "username" => sortDesc ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
            "email" => sortDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "role" => sortDesc ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
            "islocked" => sortDesc ? query.OrderByDescending(u => u.IsLocked) : query.OrderBy(u => u.IsLocked),
            _ => sortDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
        };

        var pageNumber = Math.Max(1, parameters.PageNumber);
        var pageSize = Math.Min(Math.Max(1, parameters.PageSize), AppConstants.MaxPageSize);

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    /// <summary>
    /// 检查用户是否为管理员
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否为管理员</returns>
    public async Task<bool> IsUserAdminAsync(int userId)
    {
        return await _dbSet.AnyAsync(u =>
            u.Id == userId && u.Role == AppConstants.UserRoles.Admin);
    }

    /// <summary>
    /// 检查账户是否被锁定
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否被锁定</returns>
    public async Task<bool> IsUserLockedAsync(int userId)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        if (!user.IsLocked) return false;

        if (user.LockEndTime.HasValue && user.LockEndTime.Value < DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }
}
