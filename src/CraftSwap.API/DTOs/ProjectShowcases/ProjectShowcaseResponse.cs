namespace CraftSwap.DTOs.ProjectShowcases;

/// <summary>
/// 项目展示响应
/// </summary>
public class ProjectShowcaseResponse
{
    /// <summary>
    /// 项目ID
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
    /// 分类
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 图片URL列表
    /// </summary>
    public List<string> ImageUrls { get; set; } = new List<string>();

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// 作者ID
    /// </summary>
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>
    /// 作者用户名
    /// </summary>
    public string AuthorUsername { get; set; } = string.Empty;

    /// <summary>
    /// 作者头像
    /// </summary>
    public string? AuthorAvatarUrl { get; set; }

    /// <summary>
    /// 点赞数
    /// </summary>
    public int LikeCount { get; set; }

    /// <summary>
    /// 浏览数
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// 收藏数
    /// </summary>
    public int FavoriteCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
