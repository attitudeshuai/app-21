using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.Materials;

namespace CraftSwap.Services;

/// <summary>
/// 材料服务接口
/// </summary>
public interface IMaterialService
{
    /// <summary>
    /// 分页获取材料列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页材料响应</returns>
    Task<ApiResponse<PagedResponse<MaterialResponse>>> GetPagedAsync(MaterialQueryParameters parameters);

    /// <summary>
    /// 高级搜索材料（支持所有筛选条件、相关性排序、关键词高亮）
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页材料响应（含高亮和相关性分数）</returns>
    Task<ApiResponse<PagedResponse<MaterialResponse>>> AdvancedSearchAsync(MaterialQueryParameters parameters);

    /// <summary>
    /// 根据ID获取材料详情
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <returns>材料响应</returns>
    Task<ApiResponse<MaterialResponse>> GetByIdAsync(int id);

    /// <summary>
    /// 创建材料
    /// </summary>
    /// <param name="request">创建材料请求</param>
    /// <returns>材料响应</returns>
    Task<ApiResponse<MaterialResponse>> CreateAsync(CreateMaterialRequest request);

    /// <summary>
    /// 更新材料
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <param name="request">更新材料请求</param>
    /// <returns>材料响应</returns>
    Task<ApiResponse<MaterialResponse>> UpdateAsync(int id, UpdateMaterialRequest request);

    /// <summary>
    /// 删除材料
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <returns>操作结果</returns>
    Task<ApiResponse> DeleteAsync(int id);

    /// <summary>
    /// 更新材料状态
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <param name="status">新状态</param>
    /// <returns>材料响应</returns>
    Task<ApiResponse<MaterialResponse>> UpdateStatusAsync(int id, string status);

    /// <summary>
    /// 获取当前用户的材料列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页材料响应</returns>
    Task<ApiResponse<PagedResponse<MaterialResponse>>> GetMyMaterialsAsync(MaterialQueryParameters parameters);
}
