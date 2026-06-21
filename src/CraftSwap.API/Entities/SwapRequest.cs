using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftSwap.Entities;

/// <summary>
/// 交换请求实体
/// </summary>
[Table("SwapRequests")]
public class SwapRequest
{
    /// <summary>
    /// 交换请求主键ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 发起交换请求的用户ID
    /// </summary>
    [Required]
    public int ProposerId { get; set; }

    /// <summary>
    /// 接收交换请求的用户ID
    /// </summary>
    [Required]
    public int ReceiverId { get; set; }

    /// <summary>
    /// 发起方提供的材料ID
    /// </summary>
    [Required]
    public int OfferedMaterialId { get; set; }

    /// <summary>
    /// 发起方请求的材料ID
    /// </summary>
    [Required]
    public int RequestedMaterialId { get; set; }

    /// <summary>
    /// 交换请求附带的消息
    /// </summary>
    [MaxLength(1000)]
    public string? Message { get; set; }

    /// <summary>
    /// 交换请求状态
    /// 可选值：Pending（待处理）、Accepted（已接受）、Rejected（已拒绝）、Completed（已完成）
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
    /// 发起交换请求的用户
    /// </summary>
    [ForeignKey(nameof(ProposerId))]
    public User? Proposer { get; set; }

    /// <summary>
    /// 接收交换请求的用户
    /// </summary>
    [ForeignKey(nameof(ReceiverId))]
    public User? Receiver { get; set; }

    /// <summary>
    /// 发起方提供的材料
    /// </summary>
    [ForeignKey(nameof(OfferedMaterialId))]
    public Material? OfferedMaterial { get; set; }

    /// <summary>
    /// 发起方请求的材料
    /// </summary>
    [ForeignKey(nameof(RequestedMaterialId))]
    public Material? RequestedMaterial { get; set; }

    /// <summary>
    /// 交换请求相关的评价列表
    /// </summary>
    public ICollection<SwapReview> Reviews { get; set; } = new List<SwapReview>();
}
