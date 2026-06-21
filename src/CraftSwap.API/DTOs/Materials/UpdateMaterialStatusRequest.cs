using System.ComponentModel.DataAnnotations;

namespace CraftSwap.DTOs.Materials;

/// <summary>
/// 更新材料状态请求
/// </summary>
public class UpdateMaterialStatusRequest
{
    /// <summary>
    /// 状态
    /// </summary>
    [Required(ErrorMessage = "状态不能为空")]
    public string Status { get; set; } = string.Empty;
}
