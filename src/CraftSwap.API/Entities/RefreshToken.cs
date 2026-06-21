using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftSwap.Entities;

/// <summary>
/// 用户刷新令牌实体（会话存储）
/// </summary>
[Table("RefreshTokens")]
public class RefreshToken
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 关联的用户ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 刷新令牌值
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 关联的访问令牌JTI（用于标识旧凭证并使其失效）
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string AccessTokenJti { get; set; } = string.Empty;

    /// <summary>
    /// 令牌过期时间
    /// </summary>
    [Required]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最近使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 吊销时间（null表示未吊销）
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// 是否已被吊销
    /// </summary>
    [NotMapped]
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// 是否已过期
    /// </summary>
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// 是否有效（未过期且未被吊销）
    /// </summary>
    [NotMapped]
    public bool IsActive => !IsExpired && !IsRevoked;

    /// <summary>
    /// 导航属性：关联的用户
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
