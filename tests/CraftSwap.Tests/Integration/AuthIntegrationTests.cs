using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CraftSwap.Common;
using CraftSwap.Data;
using CraftSwap.DTOs.Auth;
using CraftSwap.DTOs.Common;
using CraftSwap.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CraftSwap.Tests.Integration;

/// <summary>
/// 认证相关集成测试（完整链路：注册/登录 -> 刷新令牌 -> 验证）
/// </summary>
public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("CraftSwapIntegrationTestDb");
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                }
            });
        });

        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// 集成测试：正常刷新令牌流程（登录后立即刷新）
    /// </summary>
    [Fact]
    public async Task RefreshToken_AfterLogin_ShouldReturnNewTokens()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "refreshtest_user1",
            Email = "refreshtest1@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            Nickname = "Refresh Test User 1"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerResult = await DeserializeResponse<AuthResponse>(registerResponse);
        registerResult!.Code.Should().Be(200);
        var originalTokens = registerResult.Data!;
        originalTokens.Token.Should().NotBeNullOrEmpty();
        originalTokens.RefreshToken.Should().NotBeNullOrEmpty();

        await Task.Delay(100);

        var refreshRequest = new RefreshTokenRequest
        {
            AccessToken = originalTokens.Token,
            RefreshToken = originalTokens.RefreshToken
        };

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = await DeserializeResponse<AuthResponse>(refreshResponse);

        refreshResult.Should().NotBeNull();
        refreshResult!.Code.Should().Be(200);
        refreshResult.Message.Should().Be("凭证刷新成功");
        refreshResult.Data.Should().NotBeNull();

        var newTokens = refreshResult.Data!;
        newTokens.Token.Should().NotBeNullOrEmpty();
        newTokens.RefreshToken.Should().NotBeNullOrEmpty();
        newTokens.ExpiresIn.Should().BeGreaterThan(0);
        newTokens.User.Should().NotBeNull();
        newTokens.User.Username.Should().Be(registerRequest.Username);
        newTokens.User.Email.Should().Be(registerRequest.Email);

        newTokens.Token.Should().NotBe(originalTokens.Token);
        newTokens.RefreshToken.Should().NotBe(originalTokens.RefreshToken);
    }

    /// <summary>
    /// 集成测试：使用已吊销的旧刷新令牌再次刷新应失败
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithRevokedRefreshToken_ShouldFail()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "refreshtest_user2",
            Email = "refreshtest2@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            Nickname = "Refresh Test User 2"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await DeserializeResponse<AuthResponse>(registerResponse);
        var originalTokens = registerResult!.Data!;

        var refreshRequest = new RefreshTokenRequest
        {
            AccessToken = originalTokens.Token,
            RefreshToken = originalTokens.RefreshToken
        };

        var firstRefreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        firstRefreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstRefreshResult = await DeserializeResponse<AuthResponse>(firstRefreshResponse);
        firstRefreshResult!.Code.Should().Be(200);

        var secondRefreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        secondRefreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondRefreshResult = await DeserializeResponse<AuthResponse>(secondRefreshResponse);

        secondRefreshResult.Should().NotBeNull();
        secondRefreshResult!.Code.Should().Be(401);
        secondRefreshResult.Message.Should().Be("刷新令牌已被吊销");
        secondRefreshResult.Data.Should().BeNull();
    }

    /// <summary>
    /// 集成测试：访问令牌和刷新令牌不匹配应失败
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithMismatchedTokens_ShouldFail()
    {
        var userARequest = new RegisterRequest
        {
            Username = "refreshtest_usera",
            Email = "refreshtesta@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            Nickname = "User A"
        };
        var userBRequest = new RegisterRequest
        {
            Username = "refreshtest_userb",
            Email = "refreshtestb@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            Nickname = "User B"
        };

        var responseA = await _client.PostAsJsonAsync("/api/auth/register", userARequest);
        var resultA = await DeserializeResponse<AuthResponse>(responseA);
        var tokensA = resultA!.Data!;

        var responseB = await _client.PostAsJsonAsync("/api/auth/register", userBRequest);
        var resultB = await DeserializeResponse<AuthResponse>(responseB);
        var tokensB = resultB!.Data!;

        var mismatchedRequest = new RefreshTokenRequest
        {
            AccessToken = tokensA.Token,
            RefreshToken = tokensB.RefreshToken
        };

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", mismatchedRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = await DeserializeResponse<AuthResponse>(refreshResponse);

        refreshResult.Should().NotBeNull();
        refreshResult!.Code.Should().Be(401);
        refreshResult.Message.Should().Be("访问令牌与刷新令牌不匹配");
        refreshResult.Data.Should().BeNull();
    }

    /// <summary>
    /// 集成测试：刷新令牌为空应返回验证错误
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithEmptyRefreshToken_ShouldFailValidation()
    {
        var refreshRequest = new RefreshTokenRequest
        {
            AccessToken = "some-dummy-token",
            RefreshToken = string.Empty
        };

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await refreshResponse.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("刷新令牌不能为空");
    }

    /// <summary>
    /// 集成测试：使用新颁发的访问令牌访问受保护接口应成功
    /// </summary>
    [Fact]
    public async Task RefreshToken_NewAccessToken_ShouldAccessProtectedEndpoint()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "refreshtest_user3",
            Email = "refreshtest3@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            Nickname = "Refresh Test User 3"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await DeserializeResponse<AuthResponse>(registerResponse);
        var originalTokens = registerResult!.Data!;

        var refreshRequest = new RefreshTokenRequest
        {
            AccessToken = originalTokens.Token,
            RefreshToken = originalTokens.RefreshToken
        };
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
        var refreshResult = await DeserializeResponse<AuthResponse>(refreshResponse);
        var newTokens = refreshResult!.Data!;

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.Token);

        var meResponse = await _client.SendAsync(requestMessage);
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var meResult = await DeserializeResponse<UserResponse>(meResponse);
        meResult!.Code.Should().Be(200);
        meResult.Data.Should().NotBeNull();
        meResult.Data!.Username.Should().Be(registerRequest.Username);
    }

    /// <summary>
    /// 集成测试：新刷新的令牌可以继续再次刷新（链式刷新）
    /// </summary>
    [Fact]
    public async Task RefreshToken_ChainedRefresh_ShouldSucceed()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "refreshtest_chain",
            Email = "refreshtestchain@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            Nickname = "Chain Test User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await DeserializeResponse<AuthResponse>(registerResponse);
        var currentTokens = registerResult!.Data!;

        for (int i = 0; i < 3; i++)
        {
            await Task.Delay(50);

            var refreshRequest = new RefreshTokenRequest
            {
                AccessToken = currentTokens.Token,
                RefreshToken = currentTokens.RefreshToken
            };

            var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);
            var refreshResult = await DeserializeResponse<AuthResponse>(refreshResponse);

            refreshResult!.Code.Should().Be(200, $"第 {i + 1} 次刷新应成功");
            refreshResult.Data.Should().NotBeNull();

            currentTokens = refreshResult.Data!;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentTokens.Token);
        var meResponse = await _client.SendAsync(requestMessage);
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// 集成测试：刷新时最近使用时间应被更新
    /// </summary>
    [Fact]
    public async Task RefreshToken_ShouldUpdateLastUsedTime()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "refreshtest_lastused",
            Email = "refreshtestlastused@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            Nickname = "LastUsed Test User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await DeserializeResponse<AuthResponse>(registerResponse);
        var originalTokens = registerResult!.Data!;

        await Task.Delay(100);

        var refreshRequest = new RefreshTokenRequest
        {
            AccessToken = originalTokens.Token,
            RefreshToken = originalTokens.RefreshToken
        };
        await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var oldRefreshToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == originalTokens.RefreshToken);

            oldRefreshToken.Should().NotBeNull();
            oldRefreshToken!.RevokedAt.Should().NotBeNull();
            oldRefreshToken.LastUsedAt.Should().NotBeNull();
            oldRefreshToken.LastUsedAt.Should().BeOnOrAfter(oldRefreshToken.CreatedAt);
        }
    }

    /// <summary>
    /// 反序列化 HTTP 响应为 ApiResponse
    /// </summary>
    private async Task<ApiResponse<T>?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
    }
}
