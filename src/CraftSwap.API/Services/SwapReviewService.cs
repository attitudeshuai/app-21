using System.Security.Claims;
using AutoMapper;
using CraftSwap.Common;
using CraftSwap.Data;
using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.SwapReviews;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Services;

public class SwapReviewService : ISwapReviewService
{
    private readonly ISwapReviewRepository _swapReviewRepository;
    private readonly ISwapRequestRepository _swapRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SwapReviewService(
        ISwapReviewRepository swapReviewRepository,
        ISwapRequestRepository swapRequestRepository,
        IUserRepository userRepository,
        AppDbContext dbContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _swapReviewRepository = swapReviewRepository;
        _swapRequestRepository = swapRequestRepository;
        _userRepository = userRepository;
        _dbContext = dbContext;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

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

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var swapReview = _mapper.Map<SwapReview>(request);
            swapReview.ReviewerId = userId.Value;
            swapReview.RevieweeId = revieweeId;
            swapReview.CreatedAt = DateTime.UtcNow;

            var createdSwapReview = await _swapReviewRepository.AddAsync(swapReview);

            var (avgRating, totalCount) = await _swapReviewRepository.GetReviewStatsByRevieweeIdAsync(revieweeId);
            await _userRepository.UpdateReviewStatsAsync(revieweeId, avgRating, totalCount);

            await transaction.CommitAsync();

            var swapReviewResponse = _mapper.Map<SwapReviewResponse>(createdSwapReview);
            return ApiResponse<SwapReviewResponse>.Success(swapReviewResponse, "创建成功");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

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

        var ratingChanged = request.Rating.HasValue && request.Rating.Value != swapReview.Rating;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            _mapper.Map(request, swapReview);

            var updatedSwapReview = await _swapReviewRepository.UpdateAsync(swapReview);

            if (ratingChanged)
            {
                var (avgRating, totalCount) = await _swapReviewRepository.GetReviewStatsByRevieweeIdAsync(swapReview.RevieweeId);
                await _userRepository.UpdateReviewStatsAsync(swapReview.RevieweeId, avgRating, totalCount);
            }

            await transaction.CommitAsync();

            var swapReviewResponse = _mapper.Map<SwapReviewResponse>(updatedSwapReview);
            return ApiResponse<SwapReviewResponse>.Success(swapReviewResponse, "更新成功");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

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

        var revieweeId = swapReview.RevieweeId;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            await _swapReviewRepository.DeleteAsync(id);

            var (avgRating, totalCount) = await _swapReviewRepository.GetReviewStatsByRevieweeIdAsync(revieweeId);
            await _userRepository.UpdateReviewStatsAsync(revieweeId, avgRating, totalCount);

            await transaction.CommitAsync();

            return ApiResponse.Success(null, "删除成功");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

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
