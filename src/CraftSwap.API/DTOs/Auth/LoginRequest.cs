using System.ComponentModel.DataAnnotations;

namespace CraftSwap.DTOs.Auth;

/// <summary>
/// 登录请求
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 邮箱或用户名
    /// </summary>
    [Required(ErrorMessage = "邮箱或用户名不能为空")]
    public string EmailOrUsername { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;
}
