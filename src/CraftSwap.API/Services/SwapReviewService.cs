using System.Security.Claims;
using AutoMapper;
using CraftSwap.Common;
using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.SwapReviews;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using Microsoft.AspNetCore.Http;

namespace CraftSwap.Services;

/// <summary>
/// 评价服务实现
/// </summary>
public class SwapReviewService : ISwapReviewService
{
    private readonly ISwapReviewRepository _swapReviewRepository;
    private readonly ISwapRequestRepository _swapRequestRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="swapReviewRepository">评价仓储</param>
    /// <param name="swapRequestRepository">交换请求仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="httpContextAccessor">HTTP上下文访问器</param>
    public SwapReviewService(
        ISwapReviewRepository swapReviewRepository,
        ISwapRequestRepository swapRequestRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _swapReviewRepository = swapReviewRepository;
        _swapRequestRepository = swapRequestRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 分页获取评价列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页评价响应</returns>
    public async Task<ApiResponse<PagedResponse<SwapReviewResponse>>> GetPagedAsync(SwapReviewQueryParameters parameters)
    {
        int? reviewerId = null;
        if (!string.IsNullOrWhiteSpace(parameters.ReviewerId) && int.TryParse(parameters.ReviewerId, out var parsedReviewerId))
        {
            reviewerId = parsedReviewerId;
        }

        int? revieweeId = null;
        if (!string.IsNullOrWhiteSpace(parameters.RevieweeId) && int.TryParse(parameters.RevieweeId, out var parsedRevieweeId))
        {
            revieweeId = parsedRevieweeId;
        }

        var (swapReviews, totalCount) = await _swapReviewRepository.GetPagedWithFiltersAsync(
            parameters.PageNumber,
            parameters.PageSize,
            parameters.SwapRequestId,
            reviewerId,
            revieweeId,
            parameters.Rating,
            parameters.SortBy,
            parameters.SortDirection);
        var swapReviewResponses = _mapper.Map<List<SwapReviewResponse>>(swapReviews);

        var pagedResponse = new PagedResponse<SwapReviewResponse>
        {
            Items = swapReviewResponses,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
        };

        return ApiResponse<PagedResponse<SwapReviewResponse>>.Success(pagedResponse, "获取成功");
    }

    /// <summary>
    /// 根据ID获取评价详情
    /// </summary>
    /// <param name="id">评价ID</param>
    /// <returns>评价响应</returns>
    public async Task<ApiResponse<SwapReviewResponse>> GetByIdAsync(int id)
    {
        var swapReview = await _swapReviewRepository.GetByIdAsync(id);
        if (swapReview == null)
        {
            return ApiResponse<SwapReviewResponse>.Fail("评价不存在", 404);
        }

        var swapReviewResponse = _mapper.Map<SwapReviewResponse>(swapReview);
        return ApiResponse<SwapReviewResponse>.Success(swapReviewResponse, "获取成功");
    }

    /// <summary>
    /// 创建评价
    /// </summary>
    /// <param name="request">创建评价请求</param>
    /// <returns>评价响应</returns>
    public async Task<ApiResponse<SwapReviewResponse>> CreateAsync(CreateSwapReviewRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<SwapReviewResponse>.Fail("用户未登录", 401);
        }

        var swapRequest = await _swapRequestRepository.GetByIdAsync(request.SwapRequestId);
        if (swapRequest == null)
        {
            return ApiResponse<SwapReviewResponse>.Fail("交换请求不存在", 404);
        }

        if (swapRequest.Status != AppConstants.SwapRequestStatus.Completed)
        {
            return ApiResponse<SwapReviewResponse>.Fail("只能对已完成的交换进行评价");
        }

        int revieweeId;
        if (swapRequest.ProposerId == userId.Value)
        {
            revieweeId = swapRequest.ReceiverId;
        }
        else if (swapRequest.ReceiverId == userId.Value)
        {
            revieweeId = swapRequest.ProposerId;
        }
        else
        {
            return ApiResponse<SwapReviewResponse>.Fail("无权限对该交换请求进行评价", 403);
        }

        var existingReview = await _swapReviewRepository.GetByRequestIdAndReviewerIdAsync(request.SwapRequestId, userId.Value);
        if (existingReview != null)
        {
            return ApiResponse<SwapReviewResponse>.Fail("每个交换请求只能评价一次");
        }

        var swapReview = _mapper.Map<SwapReview>(request);
        swapReview.ReviewerId = userId.Value;
        swapReview.RevieweeId = revieweeId;
        swapReview.CreatedAt = DateTime.UtcNow;

        var createdSwapReview = await _swapReviewRepository.AddAsync(swapReview);
        var swapReviewResponse = _mapper.Map<SwapReviewResponse>(createdSwapReview);

        return ApiResponse<SwapReviewResponse>.Success(swapReviewResponse, "创建成功");
    }

    /// <summary>
    /// 更新评价
    /// </summary>
    /// <param name="id">评价ID</param>
    /// <param name="request">更新评价请求</param>
    /// <returns>评价响应</returns>
    public async Task<ApiResponse<SwapReviewResponse>> UpdateAsync(int id, UpdateSwapReviewRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<SwapReviewResponse>.Fail("用户未登录", 401);
        }

        var swapReview = await _swapReviewRepository.GetByIdAsync(id);
        if (swapReview == null)
        {
            return ApiResponse<SwapReviewResponse>.Fail("评价不存在", 404);
        }

        if (swapReview.ReviewerId != userId.Value)
        {
            return ApiResponse<SwapReviewResponse>.Fail("无权限修改该评价", 403);
        }

        _mapper.Map(request, swapReview);

        var updatedSwapReview = await _swapReviewRepository.UpdateAsync(swapReview);
        var swapReviewResponse = _mapper.Map<SwapReviewResponse>(updatedSwapReview);

        return ApiResponse<SwapReviewResponse>.Success(swapReviewResponse, "更新成功");
    }

    /// <summary>
    /// 删除评价
    /// </summary>
    /// <param name="id">评价ID</param>
    /// <returns>操作结果</returns>
    public async Task<ApiResponse> DeleteAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse.Fail("用户未登录", 401);
        }

        var swapReview = await _swapReviewRepository.GetByIdAsync(id);
        if (swapReview == null)
        {
            return ApiResponse.Fail("评价不存在", 404);
        }

        if (swapReview.ReviewerId != userId.Value)
        {
            return ApiResponse.Fail("无权限删除该评价", 403);
        }

        await _swapReviewRepository.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
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
