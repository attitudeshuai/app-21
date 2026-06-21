using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftSwap.Entities;

/// <summary>
/// 材料实体
/// </summary>
[Table("Materials")]
public class Material
{
    /// <summary>
    /// 材料主键ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 材料所属用户ID
    /// </summary>
    [Required]
    public int OwnerId { get; set; }

    /// <summary>
    /// 材料名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 材料分类
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 材料颜色
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// 材料类型
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string MaterialType { get; set; } = string.Empty;

    /// <summary>
    /// 材料数量
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// 数量单位
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// 材料成色/状态描述
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// 材料照片
    /// </summary>
    [MaxLength(1000)]
    public string? Photos { get; set; }

    /// <summary>
    /// 材料状态
    /// 可选值：Available（可用）、Reserved（已预留）、Swapped（已交换）、Archived（已归档）
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 材料所属用户
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public User? Owner { get; set; }

    /// <summary>
    /// 作为交换提供方的交换请求列表
    /// </summary>
    public ICollection<SwapRequest> OfferedInRequests { get; set; } = new List<SwapRequest>();

    /// <summary>
    /// 作为被请求交换的交换请求列表
    /// </summary>
    public ICollection<SwapRequest> RequestedInRequests { get; set; } = new List<SwapRequest>();
}
