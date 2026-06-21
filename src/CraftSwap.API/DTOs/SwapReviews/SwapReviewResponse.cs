namespace CraftSwap.DTOs.SwapReviews;

/// <summary>
/// 交换评价响应
/// </summary>
public class SwapReviewResponse
{
    /// <summary>
    /// 评价ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 评分（1-5）
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// 评价内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 交换请求ID
    /// </summary>
    public int SwapRequestId { get; set; }

    /// <summary>
    /// 评价者ID
    /// </summary>
    public string ReviewerId { get; set; } = string.Empty;

    /// <summary>
    /// 评价者用户名
    /// </summary>
    public string ReviewerUsername { get; set; } = string.Empty;

    /// <summary>
    /// 评价者头像
    /// </summary>
    public string? ReviewerAvatarUrl { get; set; }

    /// <summary>
    /// 被评价者ID
    /// </summary>
    public string RevieweeId { get; set; } = string.Empty;

    /// <summary>
    /// 被评价者用户名
    /// </summary>
    public string RevieweeUsername { get; set; } = string.Empty;

    /// <summary>
    /// 被评价者头像
    /// </summary>
    public string? RevieweeAvatarUrl { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
