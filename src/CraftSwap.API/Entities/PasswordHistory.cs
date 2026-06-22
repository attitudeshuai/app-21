using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftSwap.Entities;

/// <summary>
/// 密码历史记录实体
/// </summary>
[Table("PasswordHistories")]
public class PasswordHistory
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 密码哈希
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间（密码设置时间）
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 导航属性：所属用户
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
