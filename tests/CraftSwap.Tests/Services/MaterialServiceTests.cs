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
    private readonly MaterialService _materialService;
    private readonly int _ownerId;

    public MaterialServiceTests()
    {
        _mockMaterialRepository = new Mock<IMaterialRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _ownerId = 1;

        _materialService = new MaterialService(
            _mockMaterialRepository.Object,
            _mockMapper.Object,
            _mockHttpContextAccessor.Object);
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
}
