using CraftSwap.DTOs.Auth;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 注册请求验证器
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>
    /// 构造函数，配置注册请求验证规则
    /// </summary>
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .Length(3, 50).WithMessage("用户名长度必须在3到50个字符之间")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("用户名只能包含字母、数字和下划线");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("邮箱格式不正确")
            .MaximumLength(100).WithMessage("邮箱长度不能超过100个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .Length(6, 100).WithMessage("密码长度必须在6到100个字符之间")
            .Matches(@"^(?=.*[a-zA-Z])(?=.*\d).+$").WithMessage("密码必须包含字母和数字");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("确认密码不能为空")
            .Equal(x => x.Password).WithMessage("两次输入的密码不一致");

        RuleFor(x => x.Nickname)
            .MaximumLength(50).WithMessage("昵称长度不能超过50个字符");

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("头像URL长度不能超过500个字符")
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl));
    }
}
