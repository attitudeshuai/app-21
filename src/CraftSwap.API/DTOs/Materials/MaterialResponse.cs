namespace CraftSwap.DTOs.Materials;

/// <summary>
/// 材料响应
/// </summary>
public class MaterialResponse
{
    /// <summary>
    /// 材料ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 高亮后的标题（包含高亮标签）
    /// </summary>
    public string? HighlightedTitle { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 高亮后的描述（包含高亮标签）
    /// </summary>
    public string? HighlightedDescription { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 图片URL列表
    /// </summary>
    public List<string> ImageUrls { get; set; } = new List<string>();

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// 高亮后的标签列表（包含高亮标签）
    /// </summary>
    public List<string>? HighlightedTags { get; set; }

    /// <summary>
    /// 发布者ID
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// 发布者用户名
    /// </summary>
    public string OwnerUsername { get; set; } = string.Empty;

    /// <summary>
    /// 发布者头像
    /// </summary>
    public string? OwnerAvatarUrl { get; set; }

    /// <summary>
    /// 浏览次数
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// 收藏次数
    /// </summary>
    public int FavoriteCount { get; set; }

    /// <summary>
    /// 相关性分数（仅在有关键词搜索时返回）
    /// </summary>
    public double? RelevanceScore { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
