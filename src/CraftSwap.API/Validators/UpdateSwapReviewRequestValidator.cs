using CraftSwap.DTOs.SwapReviews;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 更新交换评价请求验证器
/// </summary>
public class UpdateSwapReviewRequestValidator : AbstractValidator<UpdateSwapReviewRequest>
{
    /// <summary>
    /// 构造函数，配置更新交换评价请求验证规则
    /// </summary>
    public UpdateSwapReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("评分必须在1到5之间")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Content)
            .MaximumLength(1000).WithMessage("评价内容长度不能超过1000个字符")
            .When(x => !string.IsNullOrEmpty(x.Content));
    }
}
