using CraftSwap.DTOs.Auth;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 更新个人资料请求验证器
/// </summary>
public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    /// <summary>
    /// 构造函数，配置更新个人资料请求验证规则
    /// </summary>
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Nickname)
            .MaximumLength(50).WithMessage("昵称长度不能超过50个字符")
            .When(x => !string.IsNullOrEmpty(x.Nickname));

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("头像URL长度不能超过500个字符")
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl));

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("个人简介长度不能超过500个字符")
            .When(x => !string.IsNullOrEmpty(x.Bio));
    }
}
