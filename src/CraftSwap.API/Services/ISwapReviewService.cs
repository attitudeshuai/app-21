using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.SwapReviews;

namespace CraftSwap.Services;

/// <summary>
/// 评价服务接口
/// </summary>
public interface ISwapReviewService
{
    /// <summary>
    /// 分页获取评价列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页评价响应</returns>
    Task<ApiResponse<PagedResponse<SwapReviewResponse>>> GetPagedAsync(SwapReviewQueryParameters parameters);

    /// <summary>
    /// 根据ID获取评价详情
    /// </summary>
    /// <param name="id">评价ID</param>
    /// <returns>评价响应</returns>
    Task<ApiResponse<SwapReviewResponse>> GetByIdAsync(int id);

    /// <summary>
    /// 创建评价
    /// </summary>
    /// <param name="request">创建评价请求</param>
    /// <returns>评价响应</returns>
    Task<ApiResponse<SwapReviewResponse>> CreateAsync(CreateSwapReviewRequest request);

    /// <summary>
    /// 更新评价
    /// </summary>
    /// <param name="id">评价ID</param>
    /// <param name="request">更新评价请求</param>
    /// <returns>评价响应</returns>
    Task<ApiResponse<SwapReviewResponse>> UpdateAsync(int id, UpdateSwapReviewRequest request);

    /// <summary>
    /// 删除评价
    /// </summary>
    /// <param name="id">评价ID</param>
    /// <returns>操作结果</returns>
    Task<ApiResponse> DeleteAsync(int id);
}
