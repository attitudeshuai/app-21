using System.ComponentModel.DataAnnotations;

namespace CraftSwap.DTOs.SwapRequests;

/// <summary>
/// 更新交换请求请求
/// </summary>
public class UpdateSwapRequestRequest
{
    /// <summary>
    /// 标题
    /// </summary>
    [StringLength(100, MinimumLength = 2, ErrorMessage = "标题长度必须在2到100个字符之间")]
    public string? Title { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(2000, ErrorMessage = "描述长度不能超过2000个字符")]
    public string? Description { get; set; }
}
