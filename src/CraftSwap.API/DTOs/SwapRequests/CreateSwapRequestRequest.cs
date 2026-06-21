using System.ComponentModel.DataAnnotations;

namespace CraftSwap.DTOs.SwapRequests;

/// <summary>
/// 创建交换请求请求
/// </summary>
public class CreateSwapRequestRequest
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
    /// 请求者材料ID
    /// </summary>
    [Required(ErrorMessage = "请求者材料ID不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "请求者材料ID必须大于0")]
    public int RequesterMaterialId { get; set; }

    /// <summary>
    /// 被请求者材料ID
    /// </summary>
    [Required(ErrorMessage = "被请求者材料ID不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "被请求者材料ID必须大于0")]
    public int ResponderMaterialId { get; set; }
}
