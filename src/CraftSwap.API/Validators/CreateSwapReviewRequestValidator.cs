using CraftSwap.DTOs.SwapReviews;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 创建交换评价请求验证器
/// </summary>
public class CreateSwapReviewRequestValidator : AbstractValidator<CreateSwapReviewRequest>
{
    /// <summary>
    /// 构造函数，配置创建交换评价请求验证规则
    /// </summary>
    public CreateSwapReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .NotEmpty().WithMessage("评分不能为空")
            .InclusiveBetween(1, 5).WithMessage("评分必须在1到5之间");

        RuleFor(x => x.Content)
            .MaximumLength(1000).WithMessage("评价内容长度不能超过1000个字符")
            .When(x => !string.IsNullOrEmpty(x.Content));

        RuleFor(x => x.SwapRequestId)
            .NotEmpty().WithMessage("交换请求ID不能为空")
            .GreaterThan(0).WithMessage("交换请求ID必须大于0");
    }
}
