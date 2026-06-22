using System.Security.Claims;
using AutoMapper;
using CraftSwap.Common;
using CraftSwap.DTOs.Materials;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using CraftSwap.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CraftSwap.Tests.Services;

public class MaterialServiceTests
{
    private readonly Mock<IMaterialRepository> _mockMaterialRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IHighlightService> _mockHighlightService;
    private readonly MaterialService _materialService;
    private readonly int _ownerId;

    public MaterialServiceTests()
    {
        _mockMaterialRepository = new Mock<IMaterialRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHighlightService = new Mock<IHighlightService>();
        _ownerId = 1;

        _materialService = new MaterialService(
            _mockMaterialRepository.Object,
            _mockMapper.Object,
            _mockHttpContextAccessor.Object,
            _mockHighlightService.Object);
    }

    private void SetupAuthenticatedUser(int userId)
    {
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser")
        }, "TestAuth"));

        var httpContext = new DefaultHttpContext { User = claims };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    }

    [Fact]
    public async Task CreateAsync_WhenSuccessful_ShouldReturnSuccessWithMaterial()
    {
        SetupAuthenticatedUser(_ownerId);

        var request = new CreateMaterialRequest
        {
            Title = "红色毛线",
            Description = "优质羊毛毛线，全新未使用",
            Category = "毛线",
            ImageUrls = new List<string> { "https://example.com/img1.jpg" },
            Tags = new List<string> { "羊毛", "红色" }
        };

        var material = new Material
        {
            Id = 1,
            Name = request.Title,
            Condition = request.Description,
            Category = request.Category,
            OwnerId = _ownerId,
            Status = AppConstants.MaterialStatus.Available,
            CreatedAt = DateTime.UtcNow,
            Color = "红色",
            MaterialType = "羊毛",
            Quantity = 100,
            Unit = "克"
        };

        var materialResponse = new MaterialResponse
        {
            Id = material.Id,
            Title = material.Name,
            Description = material.Condition,
            Category = material.Category,
            Status = material.Status,
            OwnerId = material.OwnerId.ToString(),
            OwnerUsername = "testuser",
            CreatedAt = material.CreatedAt
        };

        _mockMapper.Setup(x => x.Map<Material>(request)).Returns(material);
        _mockMaterialRepository.Setup(x => x.AddAsync(It.IsAny<Material>())).ReturnsAsync(material);
        _mockMapper.Setup(x => x.Map<MaterialResponse>(material)).Returns(materialResponse);

        var result = await _materialService.CreateAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("创建成功");
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(material.Id);
        result.Data.Title.Should().Be(request.Title);
        result.Data.Category.Should().Be(request.Category);

        _mockMaterialRepository.Verify(x => x.AddAsync(It.Is<Material>(m =>
            m.OwnerId == _ownerId &&
            m.Status == AppConstants.MaterialStatus.Available &&
            m.Name == request.Title)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var request = new CreateMaterialRequest
        {
            Title = "红色毛线",
            Category = "毛线"
        };

        var result = await _materialService.CreateAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(401);
        result.Message.Should().Be("用户未登录");
        result.Data.Should().BeNull();

        _mockMaterialRepository.Verify(x => x.AddAsync(It.IsAny<Material>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnerDeletes_ShouldReturnSuccess()
    {
        SetupAuthenticatedUser(_ownerId);

        var materialId = 1;
        var material = new Material
        {
            Id = materialId,
            Name = "红色毛线",
            Category = "毛线",
            OwnerId = _ownerId,
            Status = AppConstants.MaterialStatus.Available,
            CreatedAt = DateTime.UtcNow,
            Color = "红色",
            MaterialType = "羊毛",
            Quantity = 100,
            Unit = "克",
            Condition = "全新"
        };

        _mockMaterialRepository.Setup(x => x.GetByIdAsync(materialId)).ReturnsAsync(material);
        _mockMaterialRepository.Setup(x => x.DeleteAsync(materialId)).ReturnsAsync(true);

        var result = await _materialService.DeleteAsync(materialId);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("删除成功");

        _mockMaterialRepository.Verify(x => x.GetByIdAsync(materialId), Times.Once);
        _mockMaterialRepository.Verify(x => x.DeleteAsync(materialId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNonOwnerDeletes_ShouldReturnForbidden()
    {
        SetupAuthenticatedUser(_ownerId);

        var materialId = 1;
        var otherUserId = 99;
        var material = new Material
        {
            Id = materialId,
            Name = "红色毛线",
            Category = "毛线",
            OwnerId = otherUserId,
            Status = AppConstants.MaterialStatus.Available,
            CreatedAt = DateTime.UtcNow,
            Color = "红色",
            MaterialType = "羊毛",
            Quantity = 100,
            Unit = "克",
            Condition = "全新"
        };

        _mockMaterialRepository.Setup(x => x.GetByIdAsync(materialId)).ReturnsAsync(material);

        var result = await _materialService.DeleteAsync(materialId);

        result.Should().NotBeNull();
        result.Code.Should().Be(403);
        result.Message.Should().Be("无权限删除该材料");

        _mockMaterialRepository.Verify(x => x.GetByIdAsync(materialId), Times.Once);
        _mockMaterialRepository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenMaterialNotFound_ShouldReturnNotFound()
    {
        SetupAuthenticatedUser(_ownerId);

        var materialId = 999;
        _mockMaterialRepository.Setup(x => x.GetByIdAsync(materialId)).ReturnsAsync((Material?)null);

        var result = await _materialService.DeleteAsync(materialId);

        result.Should().NotBeNull();
        result.Code.Should().Be(404);
        result.Message.Should().Be("材料不存在");

        _mockMaterialRepository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var result = await _materialService.DeleteAsync(1);

        result.Should().NotBeNull();
        result.Code.Should().Be(401);
        result.Message.Should().Be("用户未登录");

        _mockMaterialRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockMaterialRepository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnerUpdates_ShouldReturnSuccess()
    {
        SetupAuthenticatedUser(_ownerId);

        var materialId = 1;
        var request = new UpdateMaterialRequest
        {
            Title = "更新后的红色毛线",
            Description = "更新后的描述",
            Category = "毛线"
        };

        var existingMaterial = new Material
        {
            Id = materialId,
            Name = "红色毛线",
            Condition = "旧描述",
            Category = "毛线",
            OwnerId = _ownerId,
            Status = AppConstants.MaterialStatus.Available,
            CreatedAt = DateTime.UtcNow,
            Color = "红色",
            MaterialType = "羊毛",
            Quantity = 100,
            Unit = "克"
        };

        var updatedMaterial = new Material
        {
            Id = materialId,
            Name = request.Title,
            Condition = request.Description,
            Category = request.Category,
            OwnerId = _ownerId,
            Status = AppConstants.MaterialStatus.Available,
            CreatedAt = existingMaterial.CreatedAt,
            Color = "红色",
            MaterialType = "羊毛",
            Quantity = 100,
            Unit = "克"
        };

        var materialResponse = new MaterialResponse
        {
            Id = materialId,
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            Status = AppConstants.MaterialStatus.Available,
            OwnerId = _ownerId.ToString(),
            OwnerUsername = "testuser"
        };

        _mockMaterialRepository.Setup(x => x.GetByIdAsync(materialId)).ReturnsAsync(existingMaterial);
        _mockMapper.Setup(x => x.Map(request, existingMaterial)).Returns(updatedMaterial);
        _mockMaterialRepository.Setup(x => x.UpdateAsync(It.IsAny<Material>())).ReturnsAsync(updatedMaterial);
        _mockMapper.Setup(x => x.Map<MaterialResponse>(updatedMaterial)).Returns(materialResponse);

        var result = await _materialService.UpdateAsync(materialId, request);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("更新成功");
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be(request.Title);
        result.Data.Description.Should().Be(request.Description);

        _mockMaterialRepository.Verify(x => x.GetByIdAsync(materialId), Times.Once);
        _mockMaterialRepository.Verify(x => x.UpdateAsync(It.IsAny<Material>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNonOwnerUpdates_ShouldReturnForbidden()
    {
        SetupAuthenticatedUser(_ownerId);

        var materialId = 1;
        var otherUserId = 99;
        var request = new UpdateMaterialRequest
        {
            Title = "尝试更新标题"
        };

        var existingMaterial = new Material
        {
            Id = materialId,
            Name = "红色毛线",
            Category = "毛线",
            OwnerId = otherUserId,
            Status = AppConstants.MaterialStatus.Available,
            CreatedAt = DateTime.UtcNow,
            Color = "红色",
            MaterialType = "羊毛",
            Quantity = 100,
            Unit = "克",
            Condition = "全新"
        };

        _mockMaterialRepository.Setup(x => x.GetByIdAsync(materialId)).ReturnsAsync(existingMaterial);

        var result = await _materialService.UpdateAsync(materialId, request);

        result.Should().NotBeNull();
        result.Code.Should().Be(403);
        result.Message.Should().Be("无权限修改该材料");
        result.Data.Should().BeNull();

        _mockMaterialRepository.Verify(x => x.GetByIdAsync(materialId), Times.Once);
        _mockMaterialRepository.Verify(x => x.UpdateAsync(It.IsAny<Material>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenMaterialNotFound_ShouldReturnNotFound()
    {
        SetupAuthenticatedUser(_ownerId);

        var materialId = 999;
        var request = new UpdateMaterialRequest
        {
            Title = "更新标题"
        };

        _mockMaterialRepository.Setup(x => x.GetByIdAsync(materialId)).ReturnsAsync((Material?)null);

        var result = await _materialService.UpdateAsync(materialId, request);

        result.Should().NotBeNull();
        result.Code.Should().Be(404);
        result.Message.Should().Be("材料不存在");

        _mockMaterialRepository.Verify(x => x.UpdateAsync(It.IsAny<Material>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var request = new UpdateMaterialRequest { Title = "更新" };
        var result = await _materialService.UpdateAsync(1, request);

        result.Should().NotBeNull();
        result.Code.Should().Be(401);
        result.Message.Should().Be("用户未登录");

        _mockMaterialRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMaterialExists_ShouldReturnSuccess()
    {
        var materialId = 1;
        var material = new Material
        {
            Id = materialId,
            Name = "红色毛线",
            Category = "毛线",
            OwnerId = _ownerId,
            Status = AppConstants.MaterialStatus.Available,
            CreatedAt = DateTime.UtcNow,
            Color = "红色",
            MaterialType = "羊毛",
            Quantity = 100,
            Unit = "克",
            Condition = "全新"
        };

        var materialResponse = new MaterialResponse
        {
            Id = materialId,
            Title = material.Name,
            Category = material.Category,
            Status = material.Status,
            OwnerId = material.OwnerId.ToString(),
            OwnerUsername = "testuser",
            CreatedAt = material.CreatedAt
        };

        _mockMaterialRepository.Setup(x => x.GetByIdAsync(materialId)).ReturnsAsync(material);
        _mockMapper.Setup(x => x.Map<MaterialResponse>(material)).Returns(materialResponse);

        var result = await _materialService.GetByIdAsync(materialId);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("获取成功");
        result.Data.Should().BeEquivalentTo(materialResponse);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMaterialNotFound_ShouldReturnNotFound()
    {
        var materialId = 999;
        _mockMaterialRepository.Setup(x => x.GetByIdAsync(materialId)).ReturnsAsync((Material?)null);

        var result = await _materialService.GetByIdAsync(materialId);

        result.Should().NotBeNull();
        result.Code.Should().Be(404);
        result.Message.Should().Be("材料不存在");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenOwnerUpdates_ShouldReturnSuccess()
    {
        SetupAuthenticatedUser(_ownerId);

        var materialId = 1;
        var newStatus = AppConstants.MaterialStatus.Swapped;
        var material = new Material
        {
            Id = materialId,
            Name = "红色毛线",
            Category = "毛线",
            OwnerId = _ownerId,
            Status = AppConstants.MaterialStatus.Available,
            CreatedAt = DateTime.UtcNow,
            Color = "红色",
            MaterialType = "羊毛",
            Quantity = 100,
            Unit = "克",
            Condition = "全新"
        };

        var updatedMaterial = new Material
        {
            Id = material.Id,
            Name = material.Name,
            Category = material.Category,
            OwnerId = material.OwnerId,
            Status = newStatus,
            Quantity = material.Quantity,
            Unit = material.Unit,
            Condition = material.Condition
        };
        var materialResponse = new MaterialResponse
        {
            Id = materialId,
            Title = material.Name,
            Category = material.Category,
            Status = newStatus,
            OwnerId = _ownerId.ToString()
        };

        _mockMaterialRepository.Setup(x => x.GetByIdAsync(materialId)).ReturnsAsync(material);
        _mockMaterialRepository.Setup(x => x.UpdateAsync(It.IsAny<Material>())).ReturnsAsync(updatedMaterial);
        _mockMapper.Setup(x => x.Map<MaterialResponse>(updatedMaterial)).Returns(materialResponse);

        var result = await _materialService.UpdateStatusAsync(materialId, newStatus);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("状态更新成功");
        result.Data!.Status.Should().Be(newStatus);

        _mockMaterialRepository.Verify(x => x.UpdateAsync(It.Is<Material>(m => m.Status == newStatus)), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenNonOwnerUpdates_ShouldReturnForbidden()
    {
        SetupAuthenticatedUser(_ownerId);

        var materialId = 1;
        var otherUserId = 99;
        var material = new Material
        {
            Id = materialId,
            Name = "红色毛线",
            Category = "毛线",
            OwnerId = otherUserId,
            Status = AppConstants.MaterialStatus.Available,
            CreatedAt = DateTime.UtcNow,
            Color = "红色",
            MaterialType = "羊毛",
            Quantity = 100,
            Unit = "克",
            Condition = "全新"
        };

        _mockMaterialRepository.Setup(x => x.GetByIdAsync(materialId)).ReturnsAsync(material);

        var result = await _materialService.UpdateStatusAsync(materialId, AppConstants.MaterialStatus.Swapped);

        result.Should().NotBeNull();
        result.Code.Should().Be(403);
        result.Message.Should().Be("无权限修改该材料状态");
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithMultipleCategories_ShouldFilterByCategories()
    {
        var parameters = new MaterialQueryParameters
        {
            Categories = "毛线,布料",
            PageNumber = 1,
            PageSize = 10
        };

        var materials = new List<Material>
        {
            new() { Id = 1, Name = "红色毛线", Category = "毛线", CreatedAt = DateTime.UtcNow, OwnerId = _ownerId },
            new() { Id = 2, Name = "蓝色棉布", Category = "布料", CreatedAt = DateTime.UtcNow.AddHours(-1), OwnerId = _ownerId }
        };

        var searchResults = materials.Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = null
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.Is<MaterialSearchFilter>(f =>
            f.Categories.Count == 2 &&
            f.Categories.Contains("毛线") &&
            f.Categories.Contains("布料"))))
            .ReturnsAsync((searchResults, 2));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse { Id = m.Id, Title = m.Name, Category = m.Category });

        var result = await _materialService.AdvancedSearchAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);

        _mockMaterialRepository.Verify(x => x.AdvancedSearchAsync(It.IsAny<MaterialSearchFilter>()), Times.Once);
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithMultipleTags_ShouldFilterByAllTags()
    {
        var parameters = new MaterialQueryParameters
        {
            Tags = "羊毛,红色",
            PageNumber = 1,
            PageSize = 10
        };

        var materials = new List<Material>
        {
            new() { Id = 1, Name = "红色羊毛线", Category = "毛线", Tags = "羊毛,红色,保暖", CreatedAt = DateTime.UtcNow, OwnerId = _ownerId }
        };

        var searchResults = materials.Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = null
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.Is<MaterialSearchFilter>(f =>
            f.Tags.Count == 2 &&
            f.Tags.Contains("羊毛") &&
            f.Tags.Contains("红色"))))
            .ReturnsAsync((searchResults, 1));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse { Id = m.Id, Title = m.Name, Category = m.Category });

        var result = await _materialService.AdvancedSearchAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithDateRange_ShouldFilterByDateRange()
    {
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        var parameters = new MaterialQueryParameters
        {
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = 1,
            PageSize = 10
        };

        var materials = new List<Material>
        {
            new() { Id = 1, Name = "红色毛线", Category = "毛线", CreatedAt = new DateTime(2024, 6, 1), OwnerId = _ownerId }
        };

        var searchResults = materials.Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = null
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.Is<MaterialSearchFilter>(f =>
            f.StartDate == startDate &&
            f.EndDate == endDate)))
            .ReturnsAsync((searchResults, 1));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse { Id = m.Id, Title = m.Name, Category = m.Category });

        var result = await _materialService.AdvancedSearchAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithViewCountRange_ShouldFilterByViewCount()
    {
        var parameters = new MaterialQueryParameters
        {
            MinViewCount = 50,
            MaxViewCount = 200,
            PageNumber = 1,
            PageSize = 10
        };

        var materials = new List<Material>
        {
            new() { Id = 1, Name = "红色毛线", Category = "毛线", ViewCount = 100, CreatedAt = DateTime.UtcNow, OwnerId = _ownerId }
        };

        var searchResults = materials.Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = null
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.Is<MaterialSearchFilter>(f =>
            f.MinViewCount == 50 &&
            f.MaxViewCount == 200)))
            .ReturnsAsync((searchResults, 1));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse { Id = m.Id, Title = m.Name, Category = m.Category, ViewCount = m.ViewCount });

        var result = await _materialService.AdvancedSearchAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().ViewCount.Should().Be(100);
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithFavoriteCountRange_ShouldFilterByFavoriteCount()
    {
        var parameters = new MaterialQueryParameters
        {
            MinFavoriteCount = 10,
            MaxFavoriteCount = 50,
            PageNumber = 1,
            PageSize = 10
        };

        var materials = new List<Material>
        {
            new() { Id = 1, Name = "红色毛线", Category = "毛线", FavoriteCount = 25, CreatedAt = DateTime.UtcNow, OwnerId = _ownerId }
        };

        var searchResults = materials.Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = null
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.Is<MaterialSearchFilter>(f =>
            f.MinFavoriteCount == 10 &&
            f.MaxFavoriteCount == 50)))
            .ReturnsAsync((searchResults, 1));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse { Id = m.Id, Title = m.Name, Category = m.Category, FavoriteCount = m.FavoriteCount });

        var result = await _materialService.AdvancedSearchAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().FavoriteCount.Should().Be(25);
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithSearchKeyword_ShouldReturnRelevanceScore()
    {
        var parameters = new MaterialQueryParameters
        {
            SearchKeyword = "羊毛",
            SortByRelevance = true,
            PageNumber = 1,
            PageSize = 10
        };

        var materials = new List<Material>
        {
            new() { Id = 1, Title = "红色羊毛线", Description = "优质纯羊毛", Category = "毛线", Tags = "羊毛,红色", CreatedAt = DateTime.UtcNow, OwnerId = _ownerId }
        };

        var searchResults = materials.Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = 95.5
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.Is<MaterialSearchFilter>(f =>
            f.SearchKeyword == "羊毛" &&
            f.ShouldSortByRelevance)))
            .ReturnsAsync((searchResults, 1));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse
            {
                Id = m.Id,
                Title = m.Title!,
                Description = m.Description,
                Category = m.Category,
                Tags = new List<string> { "羊毛", "红色" }
            });

        _mockHighlightService.Setup(x => x.Highlight("红色羊毛线", "羊毛", "<em>", "</em>"))
            .Returns("红色<em>羊毛</em>线");

        _mockHighlightService.Setup(x => x.Highlight("优质纯羊毛", "羊毛", "<em>", "</em>"))
            .Returns("优质纯<em>羊毛</em>");

        _mockHighlightService.Setup(x => x.HighlightTags(It.IsAny<List<string>>(), "羊毛", "<em>", "</em>"))
            .Returns(new List<string> { "<em>羊毛</em>", "红色" });

        var result = await _materialService.AdvancedSearchAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);

        var item = result.Data.Items.First();
        item.RelevanceScore.Should().Be(95.5);
        item.HighlightedTitle.Should().Be("红色<em>羊毛</em>线");
        item.HighlightedDescription.Should().Be("优质纯<em>羊毛</em>");
        item.HighlightedTags.Should().BeEquivalentTo(new List<string> { "<em>羊毛</em>", "红色" });
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithCombinedFilters_ShouldApplyAllFilters()
    {
        var parameters = new MaterialQueryParameters
        {
            SearchKeyword = "羊毛",
            Categories = "毛线",
            Tags = "红色",
            MinViewCount = 50,
            MinFavoriteCount = 10,
            StartDate = new DateTime(2024, 1, 1),
            SortBy = "ViewCount",
            SortDirection = "desc",
            PageNumber = 1,
            PageSize = 10
        };

        var materials = new List<Material>
        {
            new()
            {
                Id = 1,
                Title = "红色羊毛线",
                Description = "优质纯羊毛",
                Category = "毛线",
                Tags = "羊毛,红色",
                ViewCount = 128,
                FavoriteCount = 15,
                CreatedAt = new DateTime(2024, 6, 1),
                OwnerId = _ownerId
            }
        };

        var searchResults = materials.Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = 85.0
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.Is<MaterialSearchFilter>(f =>
            f.SearchKeyword == "羊毛" &&
            f.Categories.Contains("毛线") &&
            f.Tags.Contains("红色") &&
            f.MinViewCount == 50 &&
            f.MinFavoriteCount == 10 &&
            f.StartDate == new DateTime(2024, 1, 1) &&
            f.SortBy == "ViewCount" &&
            f.SortDirection == "desc")))
            .ReturnsAsync((searchResults, 1));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse
            {
                Id = m.Id,
                Title = m.Title!,
                Description = m.Description,
                Category = m.Category,
                ViewCount = m.ViewCount,
                FavoriteCount = m.FavoriteCount,
                Tags = new List<string> { "羊毛", "红色" },
                CreatedAt = m.CreatedAt
            });

        var result = await _materialService.AdvancedSearchAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.TotalCount.Should().Be(1);

        var item = result.Data.Items.First();
        item.RelevanceScore.Should().Be(85.0);
        item.ViewCount.Should().Be(128);
        item.FavoriteCount.Should().Be(15);
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithPagination_ShouldReturnCorrectPage()
    {
        var parameters = new MaterialQueryParameters
        {
            PageNumber = 2,
            PageSize = 5
        };

        var materials = Enumerable.Range(1, 20).Select(i => new Material
        {
            Id = i,
            Name = $"材料{i}",
            Category = "毛线",
            CreatedAt = DateTime.UtcNow.AddHours(-i),
            OwnerId = _ownerId
        }).ToList();

        var searchResults = materials.Skip(5).Take(5).Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = null
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.Is<MaterialSearchFilter>(f =>
            f.PageNumber == 2 &&
            f.PageSize == 5)))
            .ReturnsAsync((searchResults, 20));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse { Id = m.Id, Title = m.Name, Category = m.Category });

        var result = await _materialService.AdvancedSearchAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(5);
        result.Data.TotalCount.Should().Be(20);
        result.Data.PageNumber.Should().Be(2);
        result.Data.PageSize.Should().Be(5);
        result.Data.TotalPages.Should().Be(4);
    }

    [Fact]
    public async Task AdvancedSearchAsync_WithHighlightDisabled_ShouldNotReturnHighlightedFields()
    {
        var parameters = new MaterialQueryParameters
        {
            SearchKeyword = "羊毛",
            EnableHighlight = false,
            PageNumber = 1,
            PageSize = 10
        };

        var materials = new List<Material>
        {
            new() { Id = 1, Title = "红色羊毛线", Description = "优质纯羊毛", Category = "毛线", CreatedAt = DateTime.UtcNow, OwnerId = _ownerId }
        };

        var searchResults = materials.Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = 75.0
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.IsAny<MaterialSearchFilter>()))
            .ReturnsAsync((searchResults, 1));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse { Id = m.Id, Title = m.Title!, Description = m.Description, Category = m.Category });

        var result = await _materialService.AdvancedSearchAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();

        var item = result.Data!.Items.First();
        item.HighlightedTitle.Should().BeNull();
        item.HighlightedDescription.Should().BeNull();
        item.HighlightedTags.Should().BeNull();

        _mockHighlightService.Verify(x => x.Highlight(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldUseAdvancedSearch()
    {
        var parameters = new MaterialQueryParameters
        {
            Category = "毛线",
            PageNumber = 1,
            PageSize = 10
        };

        var materials = new List<Material>
        {
            new() { Id = 1, Name = "红色毛线", Category = "毛线", CreatedAt = DateTime.UtcNow, OwnerId = _ownerId }
        };

        var searchResults = materials.Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = null
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.IsAny<MaterialSearchFilter>()))
            .ReturnsAsync((searchResults, 1));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse { Id = m.Id, Title = m.Name, Category = m.Category });

        var result = await _materialService.GetPagedAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);

        _mockMaterialRepository.Verify(x => x.AdvancedSearchAsync(It.IsAny<MaterialSearchFilter>()), Times.Once);
    }

    [Fact]
    public async Task GetMyMaterialsAsync_ShouldFilterByCurrentUser()
    {
        SetupAuthenticatedUser(_ownerId);

        var parameters = new MaterialQueryParameters
        {
            PageNumber = 1,
            PageSize = 10
        };

        var materials = new List<Material>
        {
            new() { Id = 1, Name = "红色毛线", Category = "毛线", CreatedAt = DateTime.UtcNow, OwnerId = _ownerId }
        };

        var searchResults = materials.Select(m => new MaterialSearchResult
        {
            Material = m,
            RelevanceScore = null
        }).ToList();

        _mockMaterialRepository.Setup(x => x.AdvancedSearchAsync(It.Is<MaterialSearchFilter>(f =>
            f.OwnerId == _ownerId)))
            .ReturnsAsync((searchResults, 1));

        _mockMapper.Setup(x => x.Map<MaterialResponse>(It.IsAny<Material>()))
            .Returns<Material>(m => new MaterialResponse { Id = m.Id, Title = m.Name, Category = m.Category });

        var result = await _materialService.GetMyMaterialsAsync(parameters);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
    }
}

