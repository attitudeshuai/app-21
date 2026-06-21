using CraftSwap.DTOs.Admin;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 锁定用户请求验证器
/// </summary>
public class LockUserRequestValidator : AbstractValidator<LockUserRequest>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public LockUserRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("用户ID必须大于0");

        RuleFor(x => x.LockDurationMinutes)
            .GreaterThan(0)
            .When(x => x.LockDurationMinutes.HasValue)
            .WithMessage("锁定时长必须大于0分钟");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("锁定原因不能为空")
            .MaximumLength(500)
            .WithMessage("锁定原因不能超过500个字符");
    }
}
