using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using CraftSwap.Common;
using CraftSwap.DTOs.Auth;
using CraftSwap.DTOs.Common;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CraftSwap.Services;

/// <summary>
/// 认证服务实现
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="jwtSettings">JWT配置</param>
    /// <param name="httpContextAccessor">HTTP上下文访问器</param>
    public AuthService(
        IUserRepository userRepository,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="request">注册请求</param>
    /// <returns>认证响应</returns>
    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingUserByUsername = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUserByUsername != null)
        {
            return ApiResponse<AuthResponse>.Fail("用户名已存在");
        }

        var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUserByEmail != null)
        {
            return ApiResponse<AuthResponse>.Fail("邮箱已被注册");
        }

        var user = _mapper.Map<User>(request);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        var createdUser = await _userRepository.AddAsync(user);

        var token = GenerateJwtToken(createdUser);
        var userResponse = _mapper.Map<UserResponse>(createdUser);

        var authResponse = new AuthResponse
        {
            User = userResponse,
            Token = token,
            RefreshToken = GenerateRefreshToken(),
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60
        };

        return ApiResponse<AuthResponse>.Success(authResponse, "注册成功");
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录请求</param>
    /// <returns>认证响应</returns>
    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        User? user = null;

        if (request.EmailOrUsername.Contains('@'))
        {
            user = await _userRepository.GetByEmailAsync(request.EmailOrUsername);
        }
        else
        {
            user = await _userRepository.GetByUsernameAsync(request.EmailOrUsername);
        }

        if (user == null)
        {
            return ApiResponse<AuthResponse>.Fail("用户名或密码错误");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return ApiResponse<AuthResponse>.Fail("用户名或密码错误");
        }

        var token = GenerateJwtToken(user);
        var userResponse = _mapper.Map<UserResponse>(user);

        var authResponse = new AuthResponse
        {
            User = userResponse,
            Token = token,
            RefreshToken = GenerateRefreshToken(),
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60
        };

        return ApiResponse<AuthResponse>.Success(authResponse, "登录成功");
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns>用户响应</returns>
    public async Task<ApiResponse<UserResponse>> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<UserResponse>.Fail("用户未登录", 401);
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("用户不存在", 404);
        }

        var userResponse = _mapper.Map<UserResponse>(user);
        return ApiResponse<UserResponse>.Success(userResponse, "获取成功");
    }

    /// <summary>
    /// 更新用户个人资料
    /// </summary>
    /// <param name="request">更新资料请求</param>
    /// <returns>用户响应</returns>
    public async Task<ApiResponse<UserResponse>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse<UserResponse>.Fail("用户未登录", 401);
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("用户不存在", 404);
        }

        _mapper.Map(request, user);
        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);
        var userResponse = _mapper.Map<UserResponse>(updatedUser);

        return ApiResponse<UserResponse>.Success(userResponse, "更新成功");
    }

    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns>JWT令牌字符串</returns>
    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <returns>刷新令牌字符串</returns>
    private string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString("N");
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
