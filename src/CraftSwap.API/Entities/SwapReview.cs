using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftSwap.Entities;

/// <summary>
/// 交换评价实体
/// </summary>
[Table("SwapReviews")]
public class SwapReview
{
    /// <summary>
    /// 评价主键ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 关联的交换请求ID
    /// </summary>
    [Required]
    public int RequestId { get; set; }

    /// <summary>
    /// 评价人ID
    /// </summary>
    [Required]
    public int ReviewerId { get; set; }

    /// <summary>
    /// 被评价人ID
    /// </summary>
    [Required]
    public int RevieweeId { get; set; }

    /// <summary>
    /// 评分
    /// </summary>
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    /// <summary>
    /// 评价内容
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 关联的交换请求
    /// </summary>
    [ForeignKey(nameof(RequestId))]
    public SwapRequest? Request { get; set; }

    /// <summary>
    /// 评价人
    /// </summary>
    [ForeignKey(nameof(ReviewerId))]
    public User? Reviewer { get; set; }

    /// <summary>
    /// 被评价人
    /// </summary>
    [ForeignKey(nameof(RevieweeId))]
    public User? Reviewee { get; set; }
}
