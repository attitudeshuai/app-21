using AutoMapper;
using CraftSwap.Common;
using CraftSwap.DTOs.Auth;
using CraftSwap.DTOs.Common;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CraftSwap.Services;

/// <summary>
/// 认证服务实现（业务协调层：组合校验、生成、存储服务）
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenValidatorService _tokenValidatorService;
    private readonly ITokenGeneratorService _tokenGeneratorService;
    private readonly IUserSessionService _userSessionService;
    private readonly ISystemLogService _systemLogService;
    private readonly IPasswordPolicyService _passwordPolicyService;
    private readonly IPasswordHistoryRepository _passwordHistoryRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="httpContextAccessor">HTTP上下文访问器</param>
    /// <param name="tokenValidatorService">令牌校验服务</param>
    /// <param name="tokenGeneratorService">令牌生成服务</param>
    /// <param name="userSessionService">用户会话服务</param>
    /// <param name="systemLogService">系统日志服务</param>
    /// <param name="passwordPolicyService">密码策略服务</param>
    /// <param name="passwordHistoryRepository">密码历史仓储</param>
    public AuthService(
        IUserRepository userRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ITokenValidatorService tokenValidatorService,
        ITokenGeneratorService tokenGeneratorService,
        IUserSessionService userSessionService,
        ISystemLogService systemLogService,
        IPasswordPolicyService passwordPolicyService,
        IPasswordHistoryRepository passwordHistoryRepository)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _tokenValidatorService = tokenValidatorService;
        _tokenGeneratorService = tokenGeneratorService;
        _userSessionService = userSessionService;
        _systemLogService = systemLogService;
        _passwordPolicyService = passwordPolicyService;
        _passwordHistoryRepository = passwordHistoryRepository;
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

        var passwordValidation = _passwordPolicyService.ValidatePassword(request.Password);
        if (!passwordValidation.IsValid)
        {
            var errorMessage = string.Join(" ", passwordValidation.Errors);
            return ApiResponse<AuthResponse>.Fail(errorMessage);
        }

        var user = _mapper.Map<User>(request);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.PasswordLastChangedAt = DateTime.UtcNow;

        var createdUser = await _userRepository.AddAsync(user);

        await _passwordHistoryRepository.AddAsync(createdUser.Id, createdUser.PasswordHash);

        var tokenResult = _tokenGeneratorService.GenerateTokenPair(createdUser);

        await _userSessionService.CreateSessionAsync(
            createdUser.Id,
            tokenResult.RefreshToken,
            tokenResult.AccessTokenJti,
            tokenResult.RefreshTokenExpiresAt);

        var userResponse = _mapper.Map<UserResponse>(createdUser);
        userResponse.PasswordSecurityTip = GetPasswordUpgradeTip(passwordValidation);

        var authResponse = new AuthResponse
        {
            User = userResponse,
            Token = tokenResult.AccessToken,
            RefreshToken = tokenResult.RefreshToken,
            ExpiresIn = tokenResult.ExpiresIn
        };

        var ipAddress = GetClientIpAddress();
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

        await _systemLogService.LogInformationAsync(
            AppConstants.EventTypes.UserRegistered,
            $"用户 {createdUser.Username} 注册成功",
            createdUser.Id,
            createdUser.Username,
            ipAddress: ipAddress,
            userAgent: userAgent);

        return ApiResponse<AuthResponse>.Success(authResponse, "注册成功");
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录请求</param>
    /// <returns>认证响应</returns>
    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var ipAddress = GetClientIpAddress();
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

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
            await _systemLogService.LogWarningAsync(
                AppConstants.EventTypes.UserLoginFailed,
                $"登录失败：用户 {request.EmailOrUsername} 不存在",
                ipAddress: ipAddress,
                userAgent: userAgent);

            return ApiResponse<AuthResponse>.Fail("用户名或密码错误");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            await _systemLogService.LogWarningAsync(
                AppConstants.EventTypes.UserLoginFailed,
                $"登录失败：用户 {user.Username} 密码错误",
                user.Id,
                user.Username,
                ipAddress: ipAddress,
                userAgent: userAgent);

            return ApiResponse<AuthResponse>.Fail("用户名或密码错误");
        }

        if (await _userRepository.IsUserLockedAsync(user.Id))
        {
            var lockMessage = user.LockEndTime.HasValue
                ? $"账户已被锁定，解锁时间：{user.LockEndTime.Value:yyyy-MM-dd HH:mm:ss}，原因：{user.LockReason}"
                : $"账户已被永久锁定，原因：{user.LockReason}";

            await _systemLogService.LogWarningAsync(
                AppConstants.EventTypes.UserLoginFailed,
                $"登录失败：用户 {user.Username} 账户被锁定",
                user.Id,
                user.Username,
                ipAddress: ipAddress,
                userAgent: userAgent);

            return ApiResponse<AuthResponse>.Fail(lockMessage, 403);
        }

        var passwordStrength = _passwordPolicyService.CalculatePasswordStrength(request.Password);
        var needsUpgrade = passwordStrength < AppConstants.PasswordPolicy.WeakPasswordScoreThreshold;

        if (needsUpgrade && user.PasswordLastChangedAt == null)
        {
            user.PasswordLastChangedAt = user.CreatedAt;
        }

        bool isPasswordExpired = AppConstants.PasswordPolicy.PasswordExpiryDays > 0
            && user.PasswordLastChangedAt.HasValue
            && (DateTime.UtcNow - user.PasswordLastChangedAt.Value).TotalDays > AppConstants.PasswordPolicy.PasswordExpiryDays;

        var tokenResult = _tokenGeneratorService.GenerateTokenPair(user);

        await _userSessionService.CreateSessionAsync(
            user.Id,
            tokenResult.RefreshToken,
            tokenResult.AccessTokenJti,
            tokenResult.RefreshTokenExpiresAt);

        var userResponse = _mapper.Map<UserResponse>(user);
        userResponse.PasswordSecurityTip = GeneratePasswordSecurityTip(
            passwordStrength,
            needsUpgrade,
            isPasswordExpired,
            user.PasswordLastChangedAt);

        var authResponse = new AuthResponse
        {
            User = userResponse,
            Token = tokenResult.AccessToken,
            RefreshToken = tokenResult.RefreshToken,
            ExpiresIn = tokenResult.ExpiresIn
        };

        await _systemLogService.LogInformationAsync(
            AppConstants.EventTypes.UserLogin,
            $"用户 {user.Username} 登录成功",
            user.Id,
            user.Username,
            ipAddress: ipAddress,
            userAgent: userAgent);

        return ApiResponse<AuthResponse>.Success(authResponse, "登录成功");
    }

    /// <summary>
    /// 刷新会话凭证
    /// 流程：校验旧访问令牌(不校验生命周期) -> 校验刷新令牌 -> 校验令牌匹配 -> 吊销旧会话 -> 更新最近使用时间 -> 生成新凭证 -> 创建新会话 -> 返回响应
    /// </summary>
    /// <param name="request">刷新令牌请求</param>
    /// <returns>认证响应（新的凭证组）</returns>
    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var accessTokenValidation = await _tokenValidatorService.ValidateAccessTokenAsync(
            request.AccessToken,
            validateLifetime: false);

        if (!accessTokenValidation.IsValid)
        {
            return ApiResponse<AuthResponse>.Fail(
                accessTokenValidation.ErrorMessage ?? "访问令牌校验失败",
                401);
        }

        var refreshTokenValidation = await _tokenValidatorService.ValidateRefreshTokenAsync(request.RefreshToken);

        if (!refreshTokenValidation.IsValid)
        {
            return ApiResponse<AuthResponse>.Fail(
                refreshTokenValidation.ErrorMessage ?? "刷新令牌校验失败",
                401);
        }

        var refreshTokenEntity = refreshTokenValidation.RefreshToken;
        if (refreshTokenEntity == null)
        {
            return ApiResponse<AuthResponse>.Fail("刷新令牌信息缺失", 401);
        }

        var isTokenPairValid = _tokenValidatorService.ValidateTokenPair(
            accessTokenValidation.Jti!,
            refreshTokenEntity);

        if (!isTokenPairValid)
        {
            return ApiResponse<AuthResponse>.Fail("访问令牌与刷新令牌不匹配", 401);
        }

        if (accessTokenValidation.UserId != refreshTokenValidation.UserId)
        {
            await _userSessionService.RevokeAllSessionsAsync(refreshTokenEntity.UserId);
            return ApiResponse<AuthResponse>.Fail("令牌所属用户不一致，安全起见已吊销所有会话", 401);
        }

        var user = await _userRepository.GetByIdAsync(refreshTokenEntity.UserId);
        if (user == null)
        {
            return ApiResponse<AuthResponse>.Fail("用户不存在", 404);
        }

        await _userSessionService.RevokeSessionAsync(refreshTokenEntity.Id);

        await _userSessionService.UpdateLastUsedAtAsync(refreshTokenEntity.Id);

        var newTokenResult = _tokenGeneratorService.GenerateTokenPair(user);

        await _userSessionService.CreateSessionAsync(
            user.Id,
            newTokenResult.RefreshToken,
            newTokenResult.AccessTokenJti,
            newTokenResult.RefreshTokenExpiresAt);

        var userResponse = _mapper.Map<UserResponse>(user);

        var authResponse = new AuthResponse
        {
            User = userResponse,
            Token = newTokenResult.AccessToken,
            RefreshToken = newTokenResult.RefreshToken,
            ExpiresIn = newTokenResult.ExpiresIn
        };

        return ApiResponse<AuthResponse>.Success(authResponse, "凭证刷新成功");
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

        var isPasswordExpired = AppConstants.PasswordPolicy.PasswordExpiryDays > 0
            && user.PasswordLastChangedAt.HasValue
            && (DateTime.UtcNow - user.PasswordLastChangedAt.Value).TotalDays > AppConstants.PasswordPolicy.PasswordExpiryDays;

        var passwordTip = GeneratePasswordSecurityTip(
            passwordStrength: 0,
            needsUpgrade: false,
            isPasswordExpired: isPasswordExpired,
            passwordLastChangedAt: user.PasswordLastChangedAt);

        userResponse.PasswordSecurityTip = passwordTip;

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
    /// 修改用户密码
    /// </summary>
    /// <param name="request">修改密码请求</param>
    /// <returns>操作结果</returns>
    public async Task<ApiResponse> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return ApiResponse.Fail("用户未登录", 401);
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return ApiResponse.Fail("用户不存在", 404);
        }

        var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
        {
            return ApiResponse.Fail("当前密码不正确");
        }

        var passwordValidation = _passwordPolicyService.ValidatePassword(request.NewPassword);
        if (!passwordValidation.IsValid)
        {
            var errorMessage = string.Join(" ", passwordValidation.Errors);
            return ApiResponse.Fail(errorMessage);
        }

        var historyHashes = await _passwordHistoryRepository.GetPasswordHashesByUserIdAsync(userId.Value);
        var recentHashes = historyHashes.Take(AppConstants.PasswordPolicy.HistoryCheckCount).ToList();

        if (_passwordPolicyService.IsPasswordInHistory(request.NewPassword, recentHashes))
        {
            return ApiResponse.Fail($"新密码不能与最近 {AppConstants.PasswordPolicy.HistoryCheckCount} 次使用过的密码相同");
        }

        var oldPasswordHash = user.PasswordHash;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordLastChangedAt = DateTime.UtcNow;
        user.PasswordMustBeChanged = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _passwordHistoryRepository.AddAsync(userId.Value, user.PasswordHash);

        await _userSessionService.RevokeAllSessionsAsync(userId.Value);

        var ipAddress = GetClientIpAddress();
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

        await _systemLogService.LogInformationAsync(
            "PasswordChanged",
            $"用户 {user.Username} 修改了密码",
            user.Id,
            user.Username,
            ipAddress: ipAddress,
            userAgent: userAgent);

        return ApiResponse.Success("密码修改成功，请重新登录");
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

    /// <summary>
    /// 获取客户端IP地址
    /// </summary>
    /// <returns>IP地址</returns>
    private string? GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// 生成密码安全提示
    /// </summary>
    private string? GeneratePasswordSecurityTip(
        int passwordStrength,
        bool needsUpgrade,
        bool isPasswordExpired,
        DateTime? passwordLastChangedAt)
    {
        if (isPasswordExpired)
        {
            return $"您的密码已过期，请尽快修改密码以确保账户安全。";
        }

        if (needsUpgrade)
        {
            var strengthDesc = _passwordPolicyService.GetStrengthDescription(passwordStrength);
            return $"当前密码强度为「{strengthDesc}」，建议设置更复杂的密码以提升账户安全性。";
        }

        if (passwordLastChangedAt.HasValue)
        {
            var daysSinceChanged = (DateTime.UtcNow - passwordLastChangedAt.Value).TotalDays;
            var daysUntilExpiry = AppConstants.PasswordPolicy.PasswordExpiryDays - daysSinceChanged;

            if (daysUntilExpiry <= 7 && daysUntilExpiry > 0)
            {
                return $"您的密码将在 {Math.Ceiling(daysUntilExpiry)} 天后过期，请及时更新。";
            }
        }

        return null;
    }

    /// <summary>
    /// 获取密码升级提示（注册时使用）
    /// </summary>
    private string? GetPasswordUpgradeTip(PasswordValidationResult validationResult)
    {
        if (validationResult.StrengthScore >= 4)
        {
            return null;
        }

        if (validationResult.Tips.Count > 0)
        {
            return $"密码强度：{validationResult.StrengthDescription}。提示：{string.Join("；", validationResult.Tips.Take(2))}";
        }

        return $"密码强度：{validationResult.StrengthDescription}。";
    }
}
