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
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ITokenValidatorService> _mockTokenValidatorService;
    private readonly Mock<ITokenGeneratorService> _mockTokenGeneratorService;
    private readonly Mock<IUserSessionService> _mockUserSessionService;
    private readonly Mock<ISystemLogService> _mockSystemLogService;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly AuthService _authService;
    private readonly JwtSettings _jwtSettings;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockTokenValidatorService = new Mock<ITokenValidatorService>();
        _mockTokenGeneratorService = new Mock<ITokenGeneratorService>();
        _mockUserSessionService = new Mock<IUserSessionService>();
        _mockSystemLogService = new Mock<ISystemLogService>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();

        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsASuperLongSecretKeyForTestingJwtToken1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };
        var mockJwtOptions = new Mock<IOptions<JwtSettings>>();
        mockJwtOptions.Setup(x => x.Value).Returns(_jwtSettings);

        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockMapper.Object,
            _mockHttpContextAccessor.Object,
            _mockTokenValidatorService.Object,
            _mockTokenGeneratorService.Object,
            _mockUserSessionService.Object,
            _mockSystemLogService.Object);
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

        var tokenResult = new TokenGenerationResult
        {
            AccessToken = "test-access-token",
            RefreshToken = "test-refresh-token",
            AccessTokenJti = "test-jti",
            ExpiresIn = 3600,
            AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _mockMapper.Setup(x => x.Map<User>(request)).Returns(user);
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<UserResponse>(user)).Returns(userResponse);
        _mockTokenGeneratorService.Setup(x => x.GenerateTokenPair(user)).Returns(tokenResult);
        _mockUserSessionService.Setup(x => x.CreateSessionAsync(
            user.Id, tokenResult.RefreshToken, tokenResult.AccessTokenJti, tokenResult.RefreshTokenExpiresAt))
            .ReturnsAsync(new RefreshToken { Id = 1 });

        var result = await _authService.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("注册成功");
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be(tokenResult.AccessToken);
        result.Data.RefreshToken.Should().Be(tokenResult.RefreshToken);
        result.Data.ExpiresIn.Should().Be(tokenResult.ExpiresIn);
        result.Data.User.Should().BeEquivalentTo(userResponse);

        _mockUserRepository.Verify(x => x.GetByUsernameAsync(request.Username), Times.Once);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _mockTokenGeneratorService.Verify(x => x.GenerateTokenPair(user), Times.Once);
        _mockUserSessionService.Verify(x => x.CreateSessionAsync(
            user.Id, tokenResult.RefreshToken, tokenResult.AccessTokenJti, tokenResult.RefreshTokenExpiresAt), Times.Once);
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

        var tokenResult = new TokenGenerationResult
        {
            AccessToken = "login-access-token",
            RefreshToken = "login-refresh-token",
            AccessTokenJti = "login-jti",
            ExpiresIn = 3600,
            AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.EmailOrUsername)).ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<UserResponse>(user)).Returns(userResponse);
        _mockTokenGeneratorService.Setup(x => x.GenerateTokenPair(user)).Returns(tokenResult);
        _mockUserSessionService.Setup(x => x.CreateSessionAsync(
            user.Id, tokenResult.RefreshToken, tokenResult.AccessTokenJti, tokenResult.RefreshTokenExpiresAt))
            .ReturnsAsync(new RefreshToken { Id = 1 });

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("登录成功");
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be(tokenResult.AccessToken);
        result.Data.RefreshToken.Should().Be(tokenResult.RefreshToken);
        result.Data.ExpiresIn.Should().Be(tokenResult.ExpiresIn);
        result.Data.User.Should().BeEquivalentTo(userResponse);

        _mockUserRepository.Verify(x => x.GetByUsernameAsync(request.EmailOrUsername), Times.Once);
        _mockUserRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _mockTokenGeneratorService.Verify(x => x.GenerateTokenPair(user), Times.Once);
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
    public async Task RefreshTokenAsync_WhenSuccessful_ShouldReturnNewTokensAndRevokeOld()
    {
        var userId = 1;
        var oldAccessToken = "old-access-token";
        var oldRefreshTokenValue = "old-refresh-token";
        var oldJti = "old-jti-123";

        var request = new RefreshTokenRequest
        {
            AccessToken = oldAccessToken,
            RefreshToken = oldRefreshTokenValue
        };

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com"
        };

        var oldRefreshTokenEntity = new RefreshToken
        {
            Id = 100,
            UserId = userId,
            Token = oldRefreshTokenValue,
            AccessTokenJti = oldJti,
            ExpiresAt = DateTime.UtcNow.AddDays(5)
        };

        var userResponse = new UserResponse
        {
            Id = userId.ToString(),
            Username = user.Username,
            Email = user.Email
        };

        var newTokenResult = new TokenGenerationResult
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            AccessTokenJti = "new-jti-456",
            ExpiresIn = 3600,
            AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockTokenValidatorService.Setup(x => x.ValidateAccessTokenAsync(oldAccessToken, false))
            .ReturnsAsync(TokenValidationResult.Success(userId, oldJti));

        _mockTokenValidatorService.Setup(x => x.ValidateRefreshTokenAsync(oldRefreshTokenValue))
            .ReturnsAsync(TokenValidationResult.Success(userId, oldJti, oldRefreshTokenEntity));

        _mockTokenValidatorService.Setup(x => x.ValidateTokenPair(oldJti, oldRefreshTokenEntity))
            .Returns(true);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        _mockTokenGeneratorService.Setup(x => x.GenerateTokenPair(user)).Returns(newTokenResult);

        _mockMapper.Setup(x => x.Map<UserResponse>(user)).Returns(userResponse);

        _mockUserSessionService.Setup(x => x.CreateSessionAsync(
            userId, newTokenResult.RefreshToken, newTokenResult.AccessTokenJti, newTokenResult.RefreshTokenExpiresAt))
            .ReturnsAsync(new RefreshToken { Id = 101 });

        var result = await _authService.RefreshTokenAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(200);
        result.Message.Should().Be("凭证刷新成功");
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be(newTokenResult.AccessToken);
        result.Data.RefreshToken.Should().Be(newTokenResult.RefreshToken);
        result.Data.ExpiresIn.Should().Be(newTokenResult.ExpiresIn);
        result.Data.User.Should().BeEquivalentTo(userResponse);

        _mockUserSessionService.Verify(x => x.RevokeSessionAsync(oldRefreshTokenEntity.Id), Times.Once);
        _mockUserSessionService.Verify(x => x.UpdateLastUsedAtAsync(oldRefreshTokenEntity.Id), Times.Once);
        _mockUserSessionService.Verify(x => x.CreateSessionAsync(
            userId, newTokenResult.RefreshToken, newTokenResult.AccessTokenJti, newTokenResult.RefreshTokenExpiresAt), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenAccessTokenValidationFails_ShouldReturnUnauthorized()
    {
        var request = new RefreshTokenRequest
        {
            AccessToken = "invalid-token",
            RefreshToken = "some-refresh-token"
        };

        _mockTokenValidatorService.Setup(x => x.ValidateAccessTokenAsync(request.AccessToken, false))
            .ReturnsAsync(TokenValidationResult.Fail("访问令牌格式无效"));

        var result = await _authService.RefreshTokenAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(401);
        result.Message.Should().Be("访问令牌格式无效");
        result.Data.Should().BeNull();

        _mockTokenValidatorService.Verify(x => x.ValidateRefreshTokenAsync(It.IsAny<string>()), Times.Never);
        _mockTokenGeneratorService.Verify(x => x.GenerateTokenPair(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenRefreshTokenRevoked_ShouldReturnUnauthorized()
    {
        var userId = 1;
        var oldJti = "old-jti";

        var request = new RefreshTokenRequest
        {
            AccessToken = "valid-access-token",
            RefreshToken = "revoked-refresh-token"
        };

        _mockTokenValidatorService.Setup(x => x.ValidateAccessTokenAsync(request.AccessToken, false))
            .ReturnsAsync(TokenValidationResult.Success(userId, oldJti));

        _mockTokenValidatorService.Setup(x => x.ValidateRefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(TokenValidationResult.Fail("刷新令牌已被吊销"));

        var result = await _authService.RefreshTokenAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(401);
        result.Message.Should().Be("刷新令牌已被吊销");
        result.Data.Should().BeNull();

        _mockTokenGeneratorService.Verify(x => x.GenerateTokenPair(It.IsAny<User>()), Times.Never);
        _mockUserSessionService.Verify(x => x.RevokeSessionAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenPairMismatch_ShouldReturnUnauthorized()
    {
        var userId = 1;
        var accessJti = "access-jti";
        var refreshJti = "different-jti";

        var request = new RefreshTokenRequest
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token-value"
        };

        var refreshTokenEntity = new RefreshToken
        {
            Id = 1,
            UserId = userId,
            Token = request.RefreshToken,
            AccessTokenJti = refreshJti
        };

        _mockTokenValidatorService.Setup(x => x.ValidateAccessTokenAsync(request.AccessToken, false))
            .ReturnsAsync(TokenValidationResult.Success(userId, accessJti));

        _mockTokenValidatorService.Setup(x => x.ValidateRefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(TokenValidationResult.Success(userId, refreshJti, refreshTokenEntity));

        _mockTokenValidatorService.Setup(x => x.ValidateTokenPair(accessJti, refreshTokenEntity))
            .Returns(false);

        var result = await _authService.RefreshTokenAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(401);
        result.Message.Should().Be("访问令牌与刷新令牌不匹配");
        result.Data.Should().BeNull();

        _mockTokenGeneratorService.Verify(x => x.GenerateTokenPair(It.IsAny<User>()), Times.Never);
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
