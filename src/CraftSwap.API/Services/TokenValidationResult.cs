using CraftSwap.Entities;

namespace CraftSwap.Services;

/// <summary>
/// 令牌校验结果
/// </summary>
public class TokenValidationResult
{
    /// <summary>
    /// 是否校验通过
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 校验失败时的错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 令牌对应的用户ID
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// 令牌的JTI（唯一标识）
    /// </summary>
    public string? Jti { get; set; }

    /// <summary>
    /// 关联的刷新令牌实体（校验刷新令牌时填充）
    /// </summary>
    public RefreshToken? RefreshToken { get; set; }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static TokenValidationResult Success(int userId, string jti, RefreshToken? refreshToken = null)
    {
        return new TokenValidationResult
        {
            IsValid = true,
            UserId = userId,
            Jti = jti,
            RefreshToken = refreshToken
        };
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static TokenValidationResult Fail(string errorMessage)
    {
        return new TokenValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}
