using System.Security.Claims;
using AutoMapper;
using CraftSwap.Common;
using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.SwapRequests;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using Microsoft.AspNetCore.Http;

namespace CraftSwap.Services;

public class SwapRequestService : ISwapRequestService
{
    private readonly ISwapRequestRepository _swapRequestRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISwapRequestStatusValidator _statusValidator;
    private readonly IPermissionService _permissionService;
    private readonly IMaterialSyncService _materialSyncService;

    public SwapRequestService(
        ISwapRequestRepository swapRequestRepository,
        IMaterialRepository materialRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ISwapRequestStatusValidator statusValidator,
        IPermissionService permissionService,
        IMaterialSyncService materialSyncService)
    {
        _swapRequestRepository = swapRequestRepository;
        _materialRepository = materialRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _statusValidator = statusValidator;
        _permissionService = permissionService;
        _materialSyncService = materialSyncService;
    }

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

        var (transitionAllowed, transitionError) = _statusValidator.ValidateTransition(swapRequest, status);
        if (!transitionAllowed)
        {
            return ApiResponse<SwapRequestResponse>.Fail(transitionError);
        }

        var (permissionAllowed, permissionError) = _permissionService.ValidateSwapRequestOperation(userId.Value, swapRequest, status);
        if (!permissionAllowed)
        {
            return ApiResponse<SwapRequestResponse>.Fail(permissionError, 403);
        }

        if (status == AppConstants.SwapRequestStatus.Completed)
        {
            var materialValidation = await _materialSyncService.ValidateMaterialsForSwapAsync(swapRequest);
            if (materialValidation.Code != 200)
            {
                return ApiResponse<SwapRequestResponse>.Fail(materialValidation.Message);
            }
        }

        swapRequest.Status = status;
        var updatedSwapRequest = await _swapRequestRepository.UpdateAsync(swapRequest);

        if (status == AppConstants.SwapRequestStatus.Completed)
        {
            await _materialSyncService.SyncMaterialsToSwappedAsync(swapRequest);
        }

        var swapRequestResponse = _mapper.Map<SwapRequestResponse>(updatedSwapRequest);
        return ApiResponse<SwapRequestResponse>.Success(swapRequestResponse, "状态更新成功");
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
