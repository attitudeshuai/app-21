using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CraftSwap.Common;
using CraftSwap.Entities;
using CraftSwap.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CraftSwap.Services;

/// <summary>
/// 令牌校验服务实现（凭证校验层）
/// </summary>
public class TokenValidatorService : ITokenValidatorService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jwtSettings">JWT配置</param>
    /// <param name="refreshTokenRepository">刷新令牌仓储</param>
    public TokenValidatorService(
        IOptions<JwtSettings> jwtSettings,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <summary>
    /// 校验访问令牌（JWT），即使令牌已过期也能提取Claims（仅用于刷新场景）
    /// </summary>
    /// <param name="accessToken">访问令牌</param>
    /// <param name="validateLifetime">是否校验生命周期</param>
    /// <returns>校验结果</returns>
    public async Task<TokenValidationResult> ValidateAccessTokenAsync(string accessToken, bool validateLifetime = true)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return TokenValidationResult.Fail("访问令牌不能为空");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = validateLifetime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out var validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var jtiClaim = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return TokenValidationResult.Fail("访问令牌中缺少有效的用户标识");
            }

            if (string.IsNullOrEmpty(jtiClaim))
            {
                return TokenValidationResult.Fail("访问令牌中缺少唯一标识");
            }

            return await Task.FromResult(TokenValidationResult.Success(userId, jtiClaim));
        }
        catch (SecurityTokenExpiredException)
        {
            if (!validateLifetime)
            {
                try
                {
                    var jwtToken = tokenHandler.ReadJwtToken(accessToken);
                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    {
                        return TokenValidationResult.Fail("访问令牌中缺少有效的用户标识");
                    }

                    if (string.IsNullOrEmpty(jtiClaim))
                    {
                        return TokenValidationResult.Fail("访问令牌中缺少唯一标识");
                    }

                    return TokenValidationResult.Success(userId, jtiClaim);
                }
                catch
                {
                    return TokenValidationResult.Fail("访问令牌格式无效");
                }
            }
            return TokenValidationResult.Fail("访问令牌已过期");
        }
        catch (SecurityTokenException)
        {
            return TokenValidationResult.Fail("访问令牌无效");
        }
        catch
        {
            return TokenValidationResult.Fail("访问令牌解析失败");
        }
    }

    /// <summary>
    /// 校验刷新令牌（含存储层校验：是否存在、是否过期、是否被吊销）
    /// </summary>
    /// <param name="refreshToken">刷新令牌值</param>
    /// <returns>校验结果</returns>
    public async Task<TokenValidationResult> ValidateRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return TokenValidationResult.Fail("刷新令牌不能为空");
        }

        var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (tokenEntity == null)
        {
            return TokenValidationResult.Fail("刷新令牌不存在");
        }

        if (tokenEntity.IsExpired)
        {
            return TokenValidationResult.Fail("刷新令牌已过期");
        }

        if (tokenEntity.IsRevoked)
        {
            return TokenValidationResult.Fail("刷新令牌已被吊销");
        }

        return TokenValidationResult.Success(tokenEntity.UserId, tokenEntity.AccessTokenJti, tokenEntity);
    }

    /// <summary>
    /// 校验访问令牌与刷新令牌是否匹配（同一对会话凭证）
    /// </summary>
    /// <param name="accessTokenJti">访问令牌的JTI</param>
    /// <param name="refreshToken">刷新令牌实体</param>
    /// <returns>是否匹配</returns>
    public bool ValidateTokenPair(string accessTokenJti, RefreshToken refreshToken)
    {
        return !string.IsNullOrEmpty(accessTokenJti)
            && !string.IsNullOrEmpty(refreshToken.AccessTokenJti)
            && accessTokenJti == refreshToken.AccessTokenJti;
    }
}
