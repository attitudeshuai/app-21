using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftSwap.Entities;

/// <summary>
/// 作品展示实体
/// </summary>
[Table("ProjectShowcases")]
public class ProjectShowcase
{
    /// <summary>
    /// 作品展示主键ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 发布作品的用户ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 作品标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 作品描述
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 使用的材料说明
    /// </summary>
    [MaxLength(1000)]
    public string? UsedMaterials { get; set; }

    /// <summary>
    /// 作品照片
    /// </summary>
    [MaxLength(1000)]
    public string? Photos { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 发布作品的用户
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
