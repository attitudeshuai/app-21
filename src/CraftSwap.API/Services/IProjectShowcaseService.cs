using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.ProjectShowcases;

namespace CraftSwap.Services;

/// <summary>
/// 作品服务接口
/// </summary>
public interface IProjectShowcaseService
{
    /// <summary>
    /// 分页获取作品列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页作品响应</returns>
    Task<ApiResponse<PagedResponse<ProjectShowcaseResponse>>> GetPagedAsync(ProjectShowcaseQueryParameters parameters);

    /// <summary>
    /// 根据ID获取作品详情
    /// </summary>
    /// <param name="id">作品ID</param>
    /// <returns>作品响应</returns>
    Task<ApiResponse<ProjectShowcaseResponse>> GetByIdAsync(int id);

    /// <summary>
    /// 创建作品
    /// </summary>
    /// <param name="request">创建作品请求</param>
    /// <returns>作品响应</returns>
    Task<ApiResponse<ProjectShowcaseResponse>> CreateAsync(CreateProjectShowcaseRequest request);

    /// <summary>
    /// 更新作品
    /// </summary>
    /// <param name="id">作品ID</param>
    /// <param name="request">更新作品请求</param>
    /// <returns>作品响应</returns>
    Task<ApiResponse<ProjectShowcaseResponse>> UpdateAsync(int id, UpdateProjectShowcaseRequest request);

    /// <summary>
    /// 删除作品
    /// </summary>
    /// <param name="id">作品ID</param>
    /// <returns>操作结果</returns>
    Task<ApiResponse> DeleteAsync(int id);

    /// <summary>
    /// 获取当前用户的作品列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页作品响应</returns>
    Task<ApiResponse<PagedResponse<ProjectShowcaseResponse>>> GetMyShowcasesAsync(ProjectShowcaseQueryParameters parameters);
}
