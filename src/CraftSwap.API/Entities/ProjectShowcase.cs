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
    /// 作品分类
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 作品标签（逗号分隔存储）
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    /// <summary>
    /// 浏览数
    /// </summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>
    /// 点赞数
    /// </summary>
    public int LikeCount { get; set; } = 0;

    /// <summary>
    /// 收藏数
    /// </summary>
    public int FavoriteCount { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 发布作品的用户
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
