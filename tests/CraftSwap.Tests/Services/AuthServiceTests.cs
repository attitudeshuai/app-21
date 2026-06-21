using System.Security.Claims;
using AutoMapper;
using CraftSwap.Common;
using CraftSwap.DTOs.Auth;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using CraftSwap.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace CraftSwap.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IOptions<JwtSettings>> _mockJwtOptions;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly AuthService _authService;
    private readonly JwtSettings _jwtSettings;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockJwtOptions = new Mock<IOptions<JwtSettings>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsASuperLongSecretKeyForTestingJwtToken1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };
        _mockJwtOptions.Setup(x => x.Value).Returns(_jwtSettings);

        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockMapper.Object,
            _mockJwtOptions.Object,
            _mockHttpContextAccessor.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenSuccessful_ShouldReturnSuccessWithToken()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            Nickname = "New User"
        };

        var user = new User
        {
            Id = 1,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var userResponse = new UserResponse
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email,
            Nickname = user.Username
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _mockMapper.Setup(x => x.Map<User>(request)).Returns(user);
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<UserResponse>(user)).Returns(userResponse);

        var result = await _authService.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("注册成功");
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.ExpiresIn.Should().Be(_jwtSettings.AccessTokenExpirationMinutes * 60);
        result.Data.User.Should().BeEquivalentTo(userResponse);

        _mockUserRepository.Verify(x => x.GetByUsernameAsync(request.Username), Times.Once);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameExists_ShouldReturnFail()
    {
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Email = "newemail@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456"
        };

        var existingUser = new User
        {
            Id = 1,
            Username = request.Username,
            Email = "old@test.com"
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync(existingUser);

        var result = await _authService.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(400);
        result.Message.Should().Be("用户名已存在");
        result.Data.Should().BeNull();

        _mockUserRepository.Verify(x => x.GetByUsernameAsync(request.Username), Times.Once);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ShouldReturnFail()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "existing@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456"
        };

        var existingUser = new User
        {
            Id = 2,
            Username = "otheruser",
            Email = request.Email
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(existingUser);

        var result = await _authService.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(400);
        result.Message.Should().Be("邮箱已被注册");
        result.Data.Should().BeNull();

        _mockUserRepository.Verify(x => x.GetByUsernameAsync(request.Username), Times.Once);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenPasswordAndConfirmPasswordMismatch_ShouldStillHashProvidedPassword()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Different@123",
            Nickname = "Test User"
        };

        var user = new User
        {
            Id = 1,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var userResponse = new UserResponse
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _mockMapper.Setup(x => x.Map<User>(request)).Returns(user);
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<UserResponse>(user)).Returns(userResponse);

        var result = await _authService.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("注册成功");

        _mockUserRepository.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.Username == request.Username &&
            u.Email == request.Email &&
            !string.IsNullOrEmpty(u.PasswordHash))), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenUsernameLoginSuccessful_ShouldReturnSuccessWithToken()
    {
        var password = "Test@123456";
        var request = new LoginRequest
        {
            EmailOrUsername = "alice",
            Password = password
        };

        var user = new User
        {
            Id = 1,
            Username = "alice",
            Email = "alice@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var userResponse = new UserResponse
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.EmailOrUsername)).ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<UserResponse>(user)).Returns(userResponse);

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("登录成功");
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.ExpiresIn.Should().Be(_jwtSettings.AccessTokenExpirationMinutes * 60);
        result.Data.User.Should().BeEquivalentTo(userResponse);

        _mockUserRepository.Verify(x => x.GetByUsernameAsync(request.EmailOrUsername), Times.Once);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenEmailLoginSuccessful_ShouldReturnSuccessWithToken()
    {
        var password = "Test@123456";
        var request = new LoginRequest
        {
            EmailOrUsername = "alice@test.com",
            Password = password
        };

        var user = new User
        {
            Id = 1,
            Username = "alice",
            Email = "alice@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var userResponse = new UserResponse
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.EmailOrUsername)).ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<UserResponse>(user)).Returns(userResponse);

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("登录成功");
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();

        _mockUserRepository.Verify(x => x.GetByEmailAsync(request.EmailOrUsername), Times.Once);
        _mockUserRepository.Verify(x => x.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldReturnFail()
    {
        var request = new LoginRequest
        {
            EmailOrUsername = "nonexistent",
            Password = "Test@123456"
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.EmailOrUsername)).ReturnsAsync((User?)null);

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(400);
        result.Message.Should().Be("用户名或密码错误");
        result.Data.Should().BeNull();

        _mockUserRepository.Verify(x => x.GetByUsernameAsync(request.EmailOrUsername), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordWrong_ShouldReturnFail()
    {
        var correctPassword = "Test@123456";
        var request = new LoginRequest
        {
            EmailOrUsername = "alice",
            Password = "WrongPassword123"
        };

        var user = new User
        {
            Id = 1,
            Username = "alice",
            Email = "alice@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.EmailOrUsername)).ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(400);
        result.Message.Should().Be("用户名或密码错误");
        result.Data.Should().BeNull();

        _mockUserRepository.Verify(x => x.GetByUsernameAsync(request.EmailOrUsername), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenAuthenticated_ShouldReturnUser()
    {
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Username = "alice",
            Email = "alice@test.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var userResponse = new UserResponse
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email
        };

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        }, "TestAuth"));

        var httpContext = new DefaultHttpContext { User = claims };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<UserResponse>(user)).Returns(userResponse);

        var result = await _authService.GetCurrentUserAsync();

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("获取成功");
        result.Data.Should().BeEquivalentTo(userResponse);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var result = await _authService.GetCurrentUserAsync();

        result.Should().NotBeNull();
        result.Code.Should().Be(401);
        result.Message.Should().Be("用户未登录");
        result.Data.Should().BeNull();
    }
}
