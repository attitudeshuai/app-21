using System.Security.Claims;
using AutoMapper;
using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.ProjectShowcases;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using Microsoft.AspNetCore.Http;

namespace CraftSwap.Services;

/// <summary>
/// 作品服务实现
/// </summary>
public class ProjectShowcaseService : IProjectShowcaseService
{
    private readonly IProjectShowcaseRepository _projectShowcaseRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="projectShowcaseRepository">作品仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="httpContextAccessor">HTTP上下文访问器</param>
    public ProjectShowcaseService(
        IProjectShowcaseRepository projectShowcaseRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _projectShowcaseRepository = projectShowcaseRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 分页获取作品列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页作品响应</returns>
    public async Task<ApiResponse<PagedResponse<ProjectShowcaseResponse>>> GetPagedAsync(ProjectShowcaseQueryParameters parameters)
    {
        var (projectShowcases, totalCount) = await _projectShowcaseRepository.GetPagedWithFiltersAsync(
            parameters.PageNumber,
            parameters.PageSize,
            parameters.UserId,
            parameters.SearchKeyword,
            parameters.Category,
            parameters.Tag,
            parameters.SortBy,
            parameters.SortDirection);
        var projectShowcaseResponses = _mapper.Map<List<ProjectShowcaseResponse>>(projectShowcases);

        var pagedResponse = new PagedResponse<ProjectShowcaseResponse>
        {
            Items = projectShowcaseResponses,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
        };

        return ApiResponse<PagedResponse<ProjectShowcaseResponse>>.Success(pagedResponse, "获取成功");
    }

    /// <summary>
    /// 根据ID获取作品详情
    /// </summary>
    /// <param name="id">作品ID</param>
    /// <returns>作品响应</returns>
    public async Task<ApiResponse<ProjectShowcaseResponse>> GetByIdAsync(int id)
    {
        var projectShowcase = await _projectShowcaseRepository.GetByIdAsync(id);
        if (projectShowcase == null)
        {
            return ApiResponse<ProjectShowcaseResponse>.Fail("作品不存在", 404);
        }

        projectShowcase.ViewCount += 1;
        await _projectShowcaseRepository.UpdateAsync(projectShowcase);

        var projectShowcaseResponse = _mapper.Map<ProjectShowcaseResponse>(projectShowcase);
        return ApiResponse<ProjectShowcaseResponse>.Success(projectShowcaseResponse, "获取成功");
    }

    /// <summary>
    /// 创建作品
    /// </summary>
    /// <param name="request">创建作品请求</param>
    /// <returns>作品响应</returns>
    public async Task<ApiResponse<ProjectShowcaseResponse>> CreateAsync(CreateProjectShowcaseRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<ProjectShowcaseResponse>.Fail("用户未登录", 401);
        }

        var projectShowcase = _mapper.Map<ProjectShowcase>(request);
        projectShowcase.UserId = userId.Value;
        projectShowcase.CreatedAt = DateTime.UtcNow;

        var createdProjectShowcase = await _projectShowcaseRepository.AddAsync(projectShowcase);
        var projectShowcaseResponse = _mapper.Map<ProjectShowcaseResponse>(createdProjectShowcase);

        return ApiResponse<ProjectShowcaseResponse>.Success(projectShowcaseResponse, "创建成功");
    }

    /// <summary>
    /// 更新作品
    /// </summary>
    /// <param name="id">作品ID</param>
    /// <param name="request">更新作品请求</param>
    /// <returns>作品响应</returns>
    public async Task<ApiResponse<ProjectShowcaseResponse>> UpdateAsync(int id, UpdateProjectShowcaseRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<ProjectShowcaseResponse>.Fail("用户未登录", 401);
        }

        var projectShowcase = await _projectShowcaseRepository.GetByIdAsync(id);
        if (projectShowcase == null)
        {
            return ApiResponse<ProjectShowcaseResponse>.Fail("作品不存在", 404);
        }

        if (projectShowcase.UserId != userId.Value)
        {
            return ApiResponse<ProjectShowcaseResponse>.Fail("无权限修改该作品", 403);
        }

        _mapper.Map(request, projectShowcase);

        var updatedProjectShowcase = await _projectShowcaseRepository.UpdateAsync(projectShowcase);
        var projectShowcaseResponse = _mapper.Map<ProjectShowcaseResponse>(updatedProjectShowcase);

        return ApiResponse<ProjectShowcaseResponse>.Success(projectShowcaseResponse, "更新成功");
    }

    /// <summary>
    /// 删除作品
    /// </summary>
    /// <param name="id">作品ID</param>
    /// <returns>操作结果</returns>
    public async Task<ApiResponse> DeleteAsync(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse.Fail("用户未登录", 401);
        }

        var projectShowcase = await _projectShowcaseRepository.GetByIdAsync(id);
        if (projectShowcase == null)
        {
            return ApiResponse.Fail("作品不存在", 404);
        }

        if (projectShowcase.UserId != userId.Value)
        {
            return ApiResponse.Fail("无权限删除该作品", 403);
        }

        await _projectShowcaseRepository.DeleteAsync(id);
        return ApiResponse.Success(null, "删除成功");
    }

    /// <summary>
    /// 获取当前用户的作品列表
    /// </summary>
    /// <param name="parameters">查询参数</param>
    /// <returns>分页作品响应</returns>
    public async Task<ApiResponse<PagedResponse<ProjectShowcaseResponse>>> GetMyShowcasesAsync(ProjectShowcaseQueryParameters parameters)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<PagedResponse<ProjectShowcaseResponse>>.Fail("用户未登录", 401);
        }

        var (projectShowcases, totalCount) = await _projectShowcaseRepository.GetPagedWithFiltersAsync(
            parameters.PageNumber,
            parameters.PageSize,
            userId.Value,
            parameters.SearchKeyword,
            parameters.Category,
            parameters.Tag,
            parameters.SortBy,
            parameters.SortDirection);
        var projectShowcaseResponses = _mapper.Map<List<ProjectShowcaseResponse>>(projectShowcases);

        var pagedResponse = new PagedResponse<ProjectShowcaseResponse>
        {
            Items = projectShowcaseResponses,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize)
        };

        return ApiResponse<PagedResponse<ProjectShowcaseResponse>>.Success(pagedResponse, "获取成功");
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
