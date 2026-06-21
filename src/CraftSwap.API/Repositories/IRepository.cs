using System.Linq.Expressions;

namespace CraftSwap.Repositories;

/// <summary>
/// 通用仓储接口
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    /// <param name="id">实体ID</param>
    /// <returns>实体对象</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// 获取所有实体
    /// </summary>
    /// <returns>实体列表</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// 分页获取实体
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDirection">排序方向（asc/desc）</param>
    /// <returns>分页后的实体列表和总数</returns>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        string? sortDirection = null);

    /// <summary>
    /// 分页获取实体（带过滤条件）
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="filter">过滤条件</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDirection">排序方向（asc/desc）</param>
    /// <returns>分页后的实体列表和总数</returns>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        string? sortBy = null,
        string? sortDirection = null);

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>添加后的实体</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>更新后的实体</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="id">实体ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 获取实体总数
    /// </summary>
    /// <returns>实体总数</returns>
    Task<int> CountAsync();

    /// <summary>
    /// 根据条件获取实体总数
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <returns>实体总数</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> filter);

    /// <summary>
    /// 根据条件查询实体
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <returns>实体列表</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter);

    /// <summary>
    /// 根据条件获取第一个实体
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <returns>实体对象</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter);

    /// <summary>
    /// 判断实体是否存在
    /// </summary>
    /// <param name="filter">过滤条件</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
}
