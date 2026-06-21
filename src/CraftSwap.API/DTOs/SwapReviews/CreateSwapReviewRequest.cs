using System.ComponentModel.DataAnnotations;

namespace CraftSwap.DTOs.SwapReviews;

/// <summary>
/// 创建交换评价请求
/// </summary>
public class CreateSwapReviewRequest
{
    /// <summary>
    /// 评分（1-5）
    /// </summary>
    [Required(ErrorMessage = "评分不能为空")]
    [Range(1, 5, ErrorMessage = "评分必须在1到5之间")]
    public int Rating { get; set; }

    /// <summary>
    /// 评价内容
    /// </summary>
    [StringLength(1000, ErrorMessage = "评价内容长度不能超过1000个字符")]
    public string? Content { get; set; }

    /// <summary>
    /// 交换请求ID
    /// </summary>
    [Required(ErrorMessage = "交换请求ID不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "交换请求ID必须大于0")]
    public int SwapRequestId { get; set; }
}
