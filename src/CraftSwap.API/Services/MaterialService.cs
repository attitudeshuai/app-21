using System.Security.Claims;
using AutoMapper;
using CraftSwap.Common;
using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.Materials;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using Microsoft.AspNetCore.Http;

namespace CraftSwap.Services;

/// <summary>
/// 材料服务实现
/// </summary>
public class MaterialService : IMaterialService
{
    private readonly IMaterialRepository _materialRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="materialRepository">材料仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="httpContextAccessor">HTTP上下文访问器</param>
    public MaterialService(
        IMaterialRepository materialRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _materialRepository = materialRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 分页获取材料列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页材料响应</returns>
    public async Task<ApiResponse<PagedResponse<MaterialResponse>>> GetPagedAsync(MaterialQueryParameters parameters)
    {
        int? ownerId = null;
        if (!string.IsNullOrWhiteSpace(parameters.OwnerId) && int.TryParse(parameters.OwnerId, out var parsedOwnerId))
        {
            ownerId = parsedOwnerId;
        }

        var (materials, totalCount) = await _materialRepository.GetPagedWithFiltersAsync(
            parameters.PageNumber,
            parameters.PageSize,
            parameters.SearchKeyword,
            parameters.Category,
            null,
            parameters.Status,
            ownerId,
            parameters.SortBy,
            parameters.SortDirection,
            parameters.Tag);
        var materialResponses = _mapper.Map<List<MaterialResponse>>(materials);

        var pagedResponse = new PagedResponse<MaterialResponse>
        {
            Items = materialResponses,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
        };

        return ApiResponse<PagedResponse<MaterialResponse>>.Success(pagedResponse, "获取成功");
    }

    /// <summary>
    /// 根据ID获取材料详情
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <returns>材料响应</returns>
    public async Task<ApiResponse<MaterialResponse>> GetByIdAsync(int id)
    {
        var material = await _materialRepository.GetByIdAsync(id);
        if (material == null)
        {
            return ApiResponse<MaterialResponse>.Fail("材料不存在", 404);
        }

        await _materialRepository.IncrementViewCountAsync(id);
        material = await _materialRepository.GetByIdAsync(id);

        var materialResponse = _mapper.Map<MaterialResponse>(material);
        return ApiResponse<MaterialResponse>.Success(materialResponse, "获取成功");
    }

    /// <summary>
    /// 创建材料
    /// </summary>
    /// <param name="request">创建材料请求</param>
    /// <returns>材料响应</returns>
    public async Task<ApiResponse<MaterialResponse>> CreateAsync(CreateMaterialRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<MaterialResponse>.Fail("用户未登录", 401);
        }

        var material = _mapper.Map<Material>(request);
        material.OwnerId = userId.Value;
        material.Status = AppConstants.MaterialStatus.Available;
        material.CreatedAt = DateTime.UtcNow;

        var createdMaterial = await _materialRepository.AddAsync(material);
        var materialResponse = _mapper.Map<MaterialResponse>(createdMaterial);

        return ApiResponse<MaterialResponse>.Success(materialResponse, "创建成功");
    }

    /// <summary>
    /// 更新材料
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <param name="request">更新材料请求</param>
    /// <returns>材料响应</returns>
    public async Task<ApiResponse<MaterialResponse>> UpdateAsync(int id, UpdateMaterialRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<MaterialResponse>.Fail("用户未登录", 401);
        }

        var material = await _materialRepository.GetByIdAsync(id);
        if (material == null)
        {
            return ApiResponse<MaterialResponse>.Fail("材料不存在", 404);
        }

        if (material.OwnerId != userId.Value)
        {
            return ApiResponse<MaterialResponse>.Fail("无权限修改该材料", 403);
        }

        _mapper.Map(request, material);

        var updatedMaterial = await _materialRepository.UpdateAsync(material);
        var materialResponse = _mapper.Map<MaterialResponse>(updatedMaterial);

        return ApiResponse<MaterialResponse>.Success(materialResponse, "更新成功");
    }

    /// <summary>
    /// 删除材料
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <returns>操作结果</returns>
    public async Task<ApiResponse> DeleteAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse.Fail("用户未登录", 401);
        }

        var material = await _materialRepository.GetByIdAsync(id);
        if (material == null)
        {
            return ApiResponse.Fail("材料不存在", 404);
        }

        if (material.OwnerId != userId.Value)
        {
            return ApiResponse.Fail("无权限删除该材料", 403);
        }

        await _materialRepository.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    /// <summary>
    /// 更新材料状态
    /// </summary>
    /// <param name="id">材料ID</param>
    /// <param name="status">新状态</param>
    /// <returns>材料响应</returns>
    public async Task<ApiResponse<MaterialResponse>> UpdateStatusAsync(int id, string status)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<MaterialResponse>.Fail("用户未登录", 401);
        }

        var material = await _materialRepository.GetByIdAsync(id);
        if (material == null)
        {
            return ApiResponse<MaterialResponse>.Fail("材料不存在", 404);
        }

        if (material.OwnerId != userId.Value)
        {
            return ApiResponse<MaterialResponse>.Fail("无权限修改该材料状态", 403);
        }

        var validStatuses = new[]
        {
            AppConstants.MaterialStatus.Available,
            AppConstants.MaterialStatus.Swapped,
            AppConstants.MaterialStatus.Offline,
            AppConstants.MaterialStatus.Pending
        };

        if (!validStatuses.Contains(status))
        {
            return ApiResponse<MaterialResponse>.Fail("无效的材料状态");
        }

        material.Status = status;
        var updatedMaterial = await _materialRepository.UpdateAsync(material);
        var materialResponse = _mapper.Map<MaterialResponse>(updatedMaterial);

        return ApiResponse<MaterialResponse>.Success(materialResponse, "状态更新成功");
    }

    /// <summary>
    /// 获取当前用户的材料列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页材料响应</returns>
    public async Task<ApiResponse<PagedResponse<MaterialResponse>>> GetMyMaterialsAsync(MaterialQueryParameters parameters)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<PagedResponse<MaterialResponse>>.Fail("用户未登录", 401);
        }

        var (materials, totalCount) = await _materialRepository.GetPagedWithFiltersAsync(
            parameters.PageNumber,
            parameters.PageSize,
            parameters.SearchKeyword,
            parameters.Category,
            null,
            parameters.Status,
            userId.Value,
            parameters.SortBy,
            parameters.SortDirection,
            parameters.Tag);
        var materialResponses = _mapper.Map<List<MaterialResponse>>(materials);

        var pagedResponse = new PagedResponse<MaterialResponse>
        {
            Items = materialResponses,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
        };

        return ApiResponse<PagedResponse<MaterialResponse>>.Success(pagedResponse, "获取成功");
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
