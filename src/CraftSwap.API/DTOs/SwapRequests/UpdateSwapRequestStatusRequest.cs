using System.ComponentModel.DataAnnotations;

namespace CraftSwap.DTOs.SwapRequests;

/// <summary>
/// 更新交换请求状态请求
/// </summary>
public class UpdateSwapRequestStatusRequest
{
    /// <summary>
    /// 状态
    /// </summary>
    [Required(ErrorMessage = "状态不能为空")]
    public string Status { get; set; } = string.Empty;
}
