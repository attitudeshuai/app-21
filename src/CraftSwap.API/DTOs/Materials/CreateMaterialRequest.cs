using System.ComponentModel.DataAnnotations;

namespace CraftSwap.DTOs.Materials;

/// <summary>
/// 创建材料请求
/// </summary>
public class CreateMaterialRequest
{
    /// <summary>
    /// 标题
    /// </summary>
    [Required(ErrorMessage = "标题不能为空")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "标题长度必须在2到100个字符之间")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(2000, ErrorMessage = "描述长度不能超过2000个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    [Required(ErrorMessage = "分类不能为空")]
    [StringLength(50, ErrorMessage = "分类长度不能超过50个字符")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 图片URL列表
    /// </summary>
    public List<string> ImageUrls { get; set; } = new List<string>();

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();
}
