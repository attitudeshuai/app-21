using CraftSwap.DTOs.SwapRequests;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 更新交换请求验证器
/// </summary>
public class UpdateSwapRequestRequestValidator : AbstractValidator<UpdateSwapRequestRequest>
{
    /// <summary>
    /// 构造函数，配置更新交换请求验证规则
    /// </summary>
    public UpdateSwapRequestRequestValidator()
    {
        RuleFor(x => x.Title)
            .Length(2, 100).WithMessage("标题长度必须在2到100个字符之间")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("描述长度不能超过2000个字符")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
