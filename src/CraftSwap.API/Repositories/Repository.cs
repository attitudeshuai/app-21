using System.Linq.Expressions;
using CraftSwap.Data;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Repositories;

/// <summary>
/// 通用仓储实现
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    protected readonly AppDbContext _context;

    /// <summary>
    /// 实体数据集
    /// </summary>
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    /// <param name="id">实体ID</param>
    /// <returns>实体对象</returns>
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// 获取所有实体
    /// </summary>
    /// <returns>实体列表</returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// 分页获取实体
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDirection">排序方向（asc/desc）</param>
    /// <returns>分页后的实体列表和总数</returns>
    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        string? sortDirection = null)
    {
        return await GetPagedAsync(pageNumber, pageSize, null, sortBy, sortDirection);
    }

    /// <summary>
    /// 分页获取实体（带过滤条件）
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="filter">过滤条件</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDirection">排序方向（asc/desc）</param>
    /// <returns>分页后的实体列表和总数</returns>
    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        string? sortBy = null,
        string? sortDirection = null)
    {
        var query = _dbSet.AsQueryable();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, sortBy, sortDirection);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>添加后的实体</returns>
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>更新后的实体</returns>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="id">实体ID</param>
    /// <returns>是否删除成功</returns>
    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
        {
            return false;
        }

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 获取实体总数
    /// </summary>
    /// <returns>实体总数</returns>
    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    /// <summary>
    /// 根据条件获取实体总数
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <returns>实体总数</returns>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbSet.CountAsync(filter);
    }

    /// <summary>
    /// 根据条件查询实体
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <returns>实体列表</returns>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbSet.Where(filter).ToListAsync();
    }

    /// <summary>
    /// 根据条件获取第一个实体
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <returns>实体对象</returns>
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbSet.FirstOrDefaultAsync(filter);
    }

    /// <summary>
    /// 判断实体是否存在
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <returns>是否存在</returns>
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbSet.AnyAsync(filter);
    }

    /// <summary>
    /// 应用排序
    /// </summary>
    /// <param name="query">查询对象</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDirection">排序方向</param>
    /// <returns>排序后的查询对象</returns>
    protected static IQueryable<T> ApplySorting(IQueryable<T> query, string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = typeof(T).GetProperty(sortBy);

        if (property == null)
        {
            return query;
        }

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = sortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true
            ? "OrderByDescending"
            : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(resultExpression);
    }
}
