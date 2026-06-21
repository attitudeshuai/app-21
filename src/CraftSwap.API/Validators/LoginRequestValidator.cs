using CraftSwap.DTOs.Auth;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 登录请求验证器
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>
    /// 构造函数，配置登录请求验证规则
    /// </summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("邮箱或用户名不能为空")
            .Length(3, 100).WithMessage("邮箱或用户名长度必须在3到100个字符之间");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .Length(6, 100).WithMessage("密码长度必须在6到100个字符之间");
    }
}
