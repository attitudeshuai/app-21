namespace CraftSwap.DTOs.SwapRequests;

/// <summary>
/// 交换请求响应
/// </summary>
public class SwapRequestResponse
{
    /// <summary>
    /// 交换请求ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 请求者材料ID
    /// </summary>
    public int RequesterMaterialId { get; set; }

    /// <summary>
    /// 请求者材料标题
    /// </summary>
    public string RequesterMaterialTitle { get; set; } = string.Empty;

    /// <summary>
    /// 被请求者材料ID
    /// </summary>
    public int ResponderMaterialId { get; set; }

    /// <summary>
    /// 被请求者材料标题
    /// </summary>
    public string ResponderMaterialTitle { get; set; } = string.Empty;

    /// <summary>
    /// 请求者ID
    /// </summary>
    public string RequesterId { get; set; } = string.Empty;

    /// <summary>
    /// 请求者用户名
    /// </summary>
    public string RequesterUsername { get; set; } = string.Empty;

    /// <summary>
    /// 请求者头像
    /// </summary>
    public string? RequesterAvatarUrl { get; set; }

    /// <summary>
    /// 被请求者ID
    /// </summary>
    public string ResponderId { get; set; } = string.Empty;

    /// <summary>
    /// 被请求者用户名
    /// </summary>
    public string ResponderUsername { get; set; } = string.Empty;

    /// <summary>
    /// 被请求者头像
    /// </summary>
    public string? ResponderAvatarUrl { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
