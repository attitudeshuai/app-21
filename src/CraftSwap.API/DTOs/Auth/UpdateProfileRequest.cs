using System.ComponentModel.DataAnnotations;

namespace CraftSwap.DTOs.Auth;

/// <summary>
/// 更新个人资料请求
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// 昵称
    /// </summary>
    [StringLength(50, ErrorMessage = "昵称长度不能超过50个字符")]
    public string? Nickname { get; set; }

    /// <summary>
    /// 头像URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 个人简介
    /// </summary>
    [StringLength(500, ErrorMessage = "个人简介长度不能超过500个字符")]
    public string? Bio { get; set; }
}
