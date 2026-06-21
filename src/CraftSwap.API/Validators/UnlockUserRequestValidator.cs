using CraftSwap.DTOs.Admin;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 解锁用户请求验证器
/// </summary>
public class UnlockUserRequestValidator : AbstractValidator<UnlockUserRequest>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public UnlockUserRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("用户ID必须大于0");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("解锁原因不能为空")
            .MaximumLength(500)
            .WithMessage("解锁原因不能超过500个字符");
    }
}
