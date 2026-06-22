using CraftSwap.DTOs.Auth;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 修改密码请求验证器
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("当前密码不能为空");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("新密码不能为空")
            .NotEqual(x => x.CurrentPassword).WithMessage("新密码不能与当前密码相同");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("确认新密码不能为空")
            .Equal(x => x.NewPassword).WithMessage("两次输入的新密码不一致");
    }
}
