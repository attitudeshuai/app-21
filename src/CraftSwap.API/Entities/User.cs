using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftSwap.Entities;

/// <summary>
/// 用户实体
/// </summary>
[Table("Users")]
public class User
{
    /// <summary>
    /// 用户主键ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 头像
    /// </summary>
    [MaxLength(512)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 用户发布的材料列表
    /// </summary>
    public ICollection<Material> Materials { get; set; } = new List<Material>();

    /// <summary>
    /// 用户发起的交换请求列表
    /// </summary>
    public ICollection<SwapRequest> ProposedSwapRequests { get; set; } = new List<SwapRequest>();

    /// <summary>
    /// 用户收到的交换请求列表
    /// </summary>
    public ICollection<SwapRequest> ReceivedSwapRequests { get; set; } = new List<SwapRequest>();

    /// <summary>
    /// 用户给出的评价列表
    /// </summary>
    public ICollection<SwapReview> GivenReviews { get; set; } = new List<SwapReview>();

    /// <summary>
    /// 用户收到的评价列表
    /// </summary>
    public ICollection<SwapReview> ReceivedReviews { get; set; } = new List<SwapReview>();

    /// <summary>
    /// 用户的作品展示列表
    /// </summary>
    public ICollection<ProjectShowcase> ProjectShowcases { get; set; } = new List<ProjectShowcase>();
}
