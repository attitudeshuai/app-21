using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CraftSwap.Common;
using CraftSwap.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CraftSwap.Services;

/// <summary>
/// 令牌生成服务实现（令牌生成层）
/// </summary>
public class TokenGeneratorService : ITokenGeneratorService
{
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jwtSettings">JWT配置</param>
    public TokenGeneratorService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// 为指定用户生成一组新的令牌（访问令牌+刷新令牌）
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns>令牌生成结果</returns>
    public TokenGenerationResult GenerateTokenPair(User user)
    {
        var jti = Guid.NewGuid().ToString();
        var accessToken = GenerateJwtAccessToken(user, jti);
        var refreshToken = GenerateRefreshTokenValue();
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        return new TokenGenerationResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenJti = jti,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    /// <summary>
    /// 生成JWT访问令牌
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <param name="jti">令牌唯一标识</param>
    /// <returns>JWT令牌字符串</returns>
    private string GenerateJwtAccessToken(User user, string jti)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
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
    /// 生成刷新令牌值（使用加密安全的GUID）
    /// </summary>
    /// <returns>刷新令牌字符串</returns>
    private static string GenerateRefreshTokenValue()
    {
        return Guid.NewGuid().ToString("N");
    }
}
