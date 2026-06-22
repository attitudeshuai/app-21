using System.ComponentModel.DataAnnotations;

namespace CraftSwap.DTOs.Auth;

/// <summary>
/// 修改密码请求
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// 当前密码
    /// </summary>
    [Required(ErrorMessage = "当前密码不能为空")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密码
    /// </summary>
    [Required(ErrorMessage = "新密码不能为空")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 确认新密码
    /// </summary>
    [Required(ErrorMessage = "确认新密码不能为空")]
    [Compare("NewPassword", ErrorMessage = "两次输入的新密码不一致")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
