using System.Security.Claims;
using AutoMapper;
using CraftSwap.Common;
using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.SwapRequests;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using Microsoft.AspNetCore.Http;

namespace CraftSwap.Services;

/// <summary>
/// 交换请求服务实现
/// </summary>
public class SwapRequestService : ISwapRequestService
{
    private readonly ISwapRequestRepository _swapRequestRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="swapRequestRepository">交换请求仓储</param>
    /// <param name="materialRepository">材料仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="httpContextAccessor">HTTP上下文访问器</param>
    public SwapRequestService(
        ISwapRequestRepository swapRequestRepository,
        IMaterialRepository materialRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _swapRequestRepository = swapRequestRepository;
        _materialRepository = materialRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 分页获取交换请求列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页交换请求响应</returns>
    public async Task<ApiResponse<PagedResponse<SwapRequestResponse>>> GetPagedAsync(SwapRequestQueryParameters parameters)
    {
        int? proposerId = null;
        if (!string.IsNullOrWhiteSpace(parameters.RequesterId) && int.TryParse(parameters.RequesterId, out var parsedProposerId))
        {
            proposerId = parsedProposerId;
        }

        int? receiverId = null;
        if (!string.IsNullOrWhiteSpace(parameters.ResponderId) && int.TryParse(parameters.ResponderId, out var parsedReceiverId))
        {
            receiverId = parsedReceiverId;
        }

        var (swapRequests, totalCount) = await _swapRequestRepository.GetPagedWithFiltersAsync(
            parameters.PageNumber,
            parameters.PageSize,
            proposerId,
            receiverId,
            parameters.Status,
            parameters.SortBy,
            parameters.SortDirection);
        var swapRequestResponses = _mapper.Map<List<SwapRequestResponse>>(swapRequests);

        var pagedResponse = new PagedResponse<SwapRequestResponse>
        {
            Items = swapRequestResponses,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
        };

        return ApiResponse<PagedResponse<SwapRequestResponse>>.Success(pagedResponse, "获取成功");
    }

    /// <summary>
    /// 根据ID获取交换请求详情
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <returns>交换请求响应</returns>
    public async Task<ApiResponse<SwapRequestResponse>> GetByIdAsync(int id)
    {
        var swapRequest = await _swapRequestRepository.GetByIdAsync(id);
        if (swapRequest == null)
        {
            return ApiResponse<SwapRequestResponse>.Fail("交换请求不存在", 404);
        }

        var swapRequestResponse = _mapper.Map<SwapRequestResponse>(swapRequest);
        return ApiResponse<SwapRequestResponse>.Success(swapRequestResponse, "获取成功");
    }

    /// <summary>
    /// 创建交换请求
    /// </summary>
    /// <param name="request">创建交换请求请求</param>
    /// <returns>交换请求响应</returns>
    public async Task<ApiResponse<SwapRequestResponse>> CreateAsync(CreateSwapRequestRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<SwapRequestResponse>.Fail("用户未登录", 401);
        }

        var offeredMaterial = await _materialRepository.GetByIdAsync(request.RequesterMaterialId);
        if (offeredMaterial == null)
        {
            return ApiResponse<SwapRequestResponse>.Fail("提供的材料不存在", 404);
        }

        if (offeredMaterial.OwnerId != userId.Value)
        {
            return ApiResponse<SwapRequestResponse>.Fail("无权限使用该材料", 403);
        }

        if (offeredMaterial.Status != AppConstants.MaterialStatus.Available)
        {
            return ApiResponse<SwapRequestResponse>.Fail("提供的材料状态不可用");
        }

        var requestedMaterial = await _materialRepository.GetByIdAsync(request.ResponderMaterialId);
        if (requestedMaterial == null)
        {
            return ApiResponse<SwapRequestResponse>.Fail("请求的材料不存在", 404);
        }

        if (requestedMaterial.Status != AppConstants.MaterialStatus.Available)
        {
            return ApiResponse<SwapRequestResponse>.Fail("请求的材料状态不可用");
        }

        if (offeredMaterial.OwnerId == requestedMaterial.OwnerId)
        {
            return ApiResponse<SwapRequestResponse>.Fail("不能向自己发送交换请求");
        }

        var swapRequest = _mapper.Map<SwapRequest>(request);
        swapRequest.ProposerId = userId.Value;
        swapRequest.ReceiverId = requestedMaterial.OwnerId;
        swapRequest.Status = AppConstants.SwapRequestStatus.Pending;
        swapRequest.CreatedAt = DateTime.UtcNow;

        var createdSwapRequest = await _swapRequestRepository.AddAsync(swapRequest);
        var swapRequestResponse = _mapper.Map<SwapRequestResponse>(createdSwapRequest);

        return ApiResponse<SwapRequestResponse>.Success(swapRequestResponse, "创建成功");
    }

    /// <summary>
    /// 更新交换请求
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <param name="request">更新交换请求请求</param>
    /// <returns>交换请求响应</returns>
    public async Task<ApiResponse<SwapRequestResponse>> UpdateAsync(int id, UpdateSwapRequestRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<SwapRequestResponse>.Fail("用户未登录", 401);
        }

        var swapRequest = await _swapRequestRepository.GetByIdAsync(id);
        if (swapRequest == null)
        {
            return ApiResponse<SwapRequestResponse>.Fail("交换请求不存在", 404);
        }

        if (swapRequest.ProposerId != userId.Value)
        {
            return ApiResponse<SwapRequestResponse>.Fail("无权限修改该交换请求", 403);
        }

        if (swapRequest.Status != AppConstants.SwapRequestStatus.Pending)
        {
            return ApiResponse<SwapRequestResponse>.Fail("仅待处理状态的请求才能修改");
        }

        _mapper.Map(request, swapRequest);

        var updatedSwapRequest = await _swapRequestRepository.UpdateAsync(swapRequest);
        var swapRequestResponse = _mapper.Map<SwapRequestResponse>(updatedSwapRequest);

        return ApiResponse<SwapRequestResponse>.Success(swapRequestResponse, "更新成功");
    }

    /// <summary>
    /// 删除交换请求
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <returns>操作结果</returns>
    public async Task<ApiResponse> DeleteAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse.Fail("用户未登录", 401);
        }

        var swapRequest = await _swapRequestRepository.GetByIdAsync(id);
        if (swapRequest == null)
        {
            return ApiResponse.Fail("交换请求不存在", 404);
        }

        if (swapRequest.ProposerId != userId.Value)
        {
            return ApiResponse.Fail("无权限删除该交换请求", 403);
        }

        if (swapRequest.Status != AppConstants.SwapRequestStatus.Pending)
        {
            return ApiResponse.Fail("仅待处理状态的请求才能删除");
        }

        await _swapRequestRepository.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    /// <summary>
    /// 更新交换请求状态
    /// </summary>
    /// <param name="id">交换请求ID</param>
    /// <param name="status">新状态</param>
    /// <returns>交换请求响应</returns>
    public async Task<ApiResponse<SwapRequestResponse>> UpdateStatusAsync(int id, string status)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<SwapRequestResponse>.Fail("用户未登录", 401);
        }

        var swapRequest = await _swapRequestRepository.GetByIdAsync(id);
        if (swapRequest == null)
        {
            return ApiResponse<SwapRequestResponse>.Fail("交换请求不存在", 404);
        }

        if (swapRequest.ReceiverId != userId.Value)
        {
            return ApiResponse<SwapRequestResponse>.Fail("无权限修改该交换请求状态", 403);
        }

        var validStatuses = new[]
        {
            AppConstants.SwapRequestStatus.Accepted,
            AppConstants.SwapRequestStatus.Rejected,
            AppConstants.SwapRequestStatus.Cancelled,
            AppConstants.SwapRequestStatus.InProgress,
            AppConstants.SwapRequestStatus.Completed
        };

        if (!validStatuses.Contains(status))
        {
            return ApiResponse<SwapRequestResponse>.Fail("无效的交换请求状态");
        }

        if (swapRequest.Status == AppConstants.SwapRequestStatus.Completed ||
            swapRequest.Status == AppConstants.SwapRequestStatus.Rejected ||
            swapRequest.Status == AppConstants.SwapRequestStatus.Cancelled)
        {
            return ApiResponse<SwapRequestResponse>.Fail("该请求状态已确定，无法修改");
        }

        swapRequest.Status = status;

        if (status == AppConstants.SwapRequestStatus.Completed)
        {
            var offeredMaterial = await _materialRepository.GetByIdAsync(swapRequest.OfferedMaterialId);
            if (offeredMaterial != null)
            {
                offeredMaterial.Status = AppConstants.MaterialStatus.Swapped;
                await _materialRepository.UpdateAsync(offeredMaterial);
            }

            var requestedMaterial = await _materialRepository.GetByIdAsync(swapRequest.RequestedMaterialId);
            if (requestedMaterial != null)
            {
                requestedMaterial.Status = AppConstants.MaterialStatus.Swapped;
                await _materialRepository.UpdateAsync(requestedMaterial);
            }
        }

        var updatedSwapRequest = await _swapRequestRepository.UpdateAsync(swapRequest);
        var swapRequestResponse = _mapper.Map<SwapRequestResponse>(updatedSwapRequest);

        return ApiResponse<SwapRequestResponse>.Success(swapRequestResponse, "状态更新成功");
    }

    /// <summary>
    /// 获取当前用户ID
    /// </summary>
    /// <returns>用户ID，未登录返回null</returns>
    private int? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
