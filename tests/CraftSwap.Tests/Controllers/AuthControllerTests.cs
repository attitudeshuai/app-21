using CraftSwap.Controllers;
using CraftSwap.DTOs.Auth;
using CraftSwap.DTOs.Common;
using CraftSwap.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Reflection;

namespace CraftSwap.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Register_WhenCalled_ReturnsOkObjectResult()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            Nickname = "New User"
        };

        var authResponse = new AuthResponse
        {
            User = new UserResponse
            {
                Id = "1",
                Username = request.Username,
                Email = request.Email,
                Nickname = request.Nickname
            },
            Token = "test-jwt-token",
            RefreshToken = "test-refresh-token",
            ExpiresIn = 3600
        };

        var expectedResponse = ApiResponse<AuthResponse>.Success(authResponse, "注册成功");

        _mockAuthService
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Register(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var actualResponse = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        actualResponse.Code.Should().Be(200);
        actualResponse.Message.Should().Be("注册成功");
        actualResponse.Data.Should().NotBeNull();
        actualResponse.Data!.Token.Should().Be("test-jwt-token");
        actualResponse.Data.User.Username.Should().Be(request.Username);

        _mockAuthService.Verify(x => x.RegisterAsync(request), Times.Once);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsOkWithError()
    {
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Email = "new@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456"
        };

        var expectedResponse = ApiResponse<AuthResponse>.Fail("用户名已存在");

        _mockAuthService
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Register(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var actualResponse = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        actualResponse.Code.Should().Be(400);
        actualResponse.Message.Should().Be("用户名已存在");
        actualResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task Login_WhenSuccessful_ReturnsOkWithToken()
    {
        var request = new LoginRequest
        {
            EmailOrUsername = "alice",
            Password = "Test@123456"
        };

        var authResponse = new AuthResponse
        {
            User = new UserResponse
            {
                Id = "1",
                Username = "alice",
                Email = "alice@test.com"
            },
            Token = "valid-jwt-token-for-alice",
            RefreshToken = "refresh-token-alice",
            ExpiresIn = 3600
        };

        var expectedResponse = ApiResponse<AuthResponse>.Success(authResponse, "登录成功");

        _mockAuthService
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Login(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var actualResponse = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        actualResponse.Code.Should().Be(200);
        actualResponse.Message.Should().Be("登录成功");
        actualResponse.Data.Should().NotBeNull();
        actualResponse.Data!.Token.Should().Be("valid-jwt-token-for-alice");
        actualResponse.Data.User.Username.Should().Be("alice");
        actualResponse.Data.ExpiresIn.Should().Be(3600);

        _mockAuthService.Verify(x => x.LoginAsync(request), Times.Once);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsOkWithError()
    {
        var request = new LoginRequest
        {
            EmailOrUsername = "alice",
            Password = "WrongPassword"
        };

        var expectedResponse = ApiResponse<AuthResponse>.Fail("用户名或密码错误");

        _mockAuthService
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Login(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var actualResponse = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        actualResponse.Code.Should().Be(400);
        actualResponse.Message.Should().Be("用户名或密码错误");
        actualResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetMe_WhenAuthenticated_ReturnsOkWithUser()
    {
        var userId = 1;
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "alice")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };

        var userResponse = new UserResponse
        {
            Id = userId.ToString(),
            Username = "alice",
            Email = "alice@test.com",
            Nickname = "Alice"
        };

        var expectedResponse = ApiResponse<UserResponse>.Success(userResponse, "获取成功");

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GetMe();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var actualResponse = okResult.Value.Should().BeOfType<ApiResponse<UserResponse>>().Subject;
        actualResponse.Code.Should().Be(200);
        actualResponse.Message.Should().Be("获取成功");
        actualResponse.Data.Should().NotBeNull();
        actualResponse.Data!.Username.Should().Be("alice");

        _mockAuthService.Verify(x => x.GetCurrentUserAsync(), Times.Once);
    }

    [Fact]
    public async Task GetMe_WhenServiceReturnsUnauthorized_ReturnsOkWithUnauthorizedCode()
    {
        var expectedResponse = ApiResponse<UserResponse>.Fail("用户未登录", 401);

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GetMe();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var actualResponse = okResult.Value.Should().BeOfType<ApiResponse<UserResponse>>().Subject;
        actualResponse.Code.Should().Be(401);
        actualResponse.Message.Should().Be("用户未登录");
        actualResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateMe_WhenAuthenticated_ReturnsOkWithUpdatedUser()
    {
        var request = new UpdateProfileRequest
        {
            Nickname = "Updated Alice",
            AvatarUrl = "https://example.com/new-avatar.jpg",
            Bio = "Hello, I'm Alice"
        };

        var userId = 1;
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "alice")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };

        var updatedUserResponse = new UserResponse
        {
            Id = userId.ToString(),
            Username = "alice",
            Email = "alice@test.com",
            Nickname = request.Nickname,
            AvatarUrl = request.AvatarUrl,
            Bio = request.Bio
        };

        var expectedResponse = ApiResponse<UserResponse>.Success(updatedUserResponse, "更新成功");

        _mockAuthService
            .Setup(x => x.UpdateProfileAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.UpdateMe(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var actualResponse = okResult.Value.Should().BeOfType<ApiResponse<UserResponse>>().Subject;
        actualResponse.Code.Should().Be(200);
        actualResponse.Message.Should().Be("更新成功");
        actualResponse.Data.Should().NotBeNull();
        actualResponse.Data!.Nickname.Should().Be(request.Nickname);
        actualResponse.Data.AvatarUrl.Should().Be(request.AvatarUrl);

        _mockAuthService.Verify(x => x.UpdateProfileAsync(request), Times.Once);
    }

    [Fact]
    public void Register_HasAllowAnonymousAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Register));
        var attributes = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), true);
        attributes.Should().NotBeEmpty();
    }

    [Fact]
    public void Login_HasAllowAnonymousAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Login));
        var attributes = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), true);
        attributes.Should().NotBeEmpty();
    }

    [Fact]
    public void GetMe_HasAuthorizeAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.GetMe));
        var attributes = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true);
        attributes.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateMe_HasAuthorizeAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.UpdateMe));
        var attributes = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true);
        attributes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RefreshToken_WhenCalled_ReturnsOkObjectResult()
    {
        var request = new RefreshTokenRequest
        {
            AccessToken = "old-access-token",
            RefreshToken = "old-refresh-token"
        };

        var authResponse = new AuthResponse
        {
            User = new UserResponse
            {
                Id = "1",
                Username = "testuser",
                Email = "test@test.com"
            },
            Token = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresIn = 3600
        };

        var expectedResponse = ApiResponse<AuthResponse>.Success(authResponse, "凭证刷新成功");

        _mockAuthService
            .Setup(x => x.RefreshTokenAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.RefreshToken(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var actualResponse = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        actualResponse.Code.Should().Be(200);
        actualResponse.Message.Should().Be("凭证刷新成功");
        actualResponse.Data.Should().NotBeNull();
        actualResponse.Data!.Token.Should().Be("new-access-token");
        actualResponse.Data.RefreshToken.Should().Be("new-refresh-token");
        actualResponse.Data.User.Username.Should().Be("testuser");

        _mockAuthService.Verify(x => x.RefreshTokenAsync(request), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WhenValidationFails_ReturnsOkWithErrorCode()
    {
        var request = new RefreshTokenRequest
        {
            AccessToken = "invalid-token",
            RefreshToken = "invalid-refresh-token"
        };

        var expectedResponse = ApiResponse<AuthResponse>.Fail("刷新令牌已被吊销", 401);

        _mockAuthService
            .Setup(x => x.RefreshTokenAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.RefreshToken(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var actualResponse = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        actualResponse.Code.Should().Be(401);
        actualResponse.Message.Should().Be("刷新令牌已被吊销");
        actualResponse.Data.Should().BeNull();
    }

    [Fact]
    public void RefreshToken_HasAllowAnonymousAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.RefreshToken));
        var attributes = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), true);
        attributes.Should().NotBeEmpty();
    }

    [Fact]
    public void AuthController_HasCorrectRoutePrefix()
    {
        var controllerType = typeof(AuthController);
        var routeAttribute = controllerType.GetCustomAttribute<RouteAttribute>();
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/auth");
    }
}
