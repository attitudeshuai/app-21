using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.SwapRequests;

namespace CraftSwap.Services;

/// <summary>
/// 交换请求服务接口
/// </summary>
public interface ISwapRequestService
{
    /// <summary>
    /// 分页获取交换请求列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页交换请求响应</returns>
    Task<ApiResponse<PagedResponse<SwapRequestResponse>>> GetPagedAsync(SwapRequestQueryParameters parameters);

    /// <summary>
    /// 根据ID获取交换请求详情
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <returns>交换请求响应</returns>
    Task<ApiResponse<SwapRequestResponse>> GetByIdAsync(int id);

    /// <summary>
    /// 创建交换请求
    /// </summary>
    /// <param name="request">创建交换请求请求</param>
    /// <returns>交换请求响应</returns>
    Task<ApiResponse<SwapRequestResponse>> CreateAsync(CreateSwapRequestRequest request);

    /// <summary>
    /// 更新交换请求
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <param name="request">更新交换请求请求</param>
    /// <returns>交换请求响应</returns>
    Task<ApiResponse<SwapRequestResponse>> UpdateAsync(int id, UpdateSwapRequestRequest request);

    /// <summary>
    /// 删除交换请求
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <returns>操作结果</returns>
    Task<ApiResponse> DeleteAsync(int id);

    /// <summary>
    /// 更新交换请求状态
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <param name="status">新状态</param>
    /// <returns>交换请求响应</returns>
    Task<ApiResponse<SwapRequestResponse>> UpdateStatusAsync(int id, string status);
}
