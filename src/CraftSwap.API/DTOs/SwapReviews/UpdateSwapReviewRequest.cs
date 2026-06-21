using System.ComponentModel.DataAnnotations;

namespace CraftSwap.DTOs.SwapReviews;

/// <summary>
/// 更新交换评价请求
/// </summary>
public class UpdateSwapReviewRequest
{
    /// <summary>
    /// 评分（1-5）
    /// </summary>
    [Range(1, 5, ErrorMessage = "评分必须在1到5之间")]
    public int? Rating { get; set; }

    /// <summary>
    /// 评价内容
    /// </summary>
    [StringLength(1000, ErrorMessage = "评价内容长度不能超过1000个字符")]
    public string? Content { get; set; }
}
