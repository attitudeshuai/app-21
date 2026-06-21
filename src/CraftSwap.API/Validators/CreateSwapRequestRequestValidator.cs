using CraftSwap.DTOs.SwapRequests;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 创建交换请求验证器
/// </summary>
public class CreateSwapRequestRequestValidator : AbstractValidator<CreateSwapRequestRequest>
{
    /// <summary>
    /// 构造函数，配置创建交换请求验证规则
    /// </summary>
    public CreateSwapRequestRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("标题不能为空")
            .Length(2, 100).WithMessage("标题长度必须在2到100个字符之间");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("描述长度不能超过2000个字符")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.RequesterMaterialId)
            .NotEmpty().WithMessage("请求者材料ID不能为空")
            .GreaterThan(0).WithMessage("请求者材料ID必须大于0");

        RuleFor(x => x.ResponderMaterialId)
            .NotEmpty().WithMessage("被请求者材料ID不能为空")
            .GreaterThan(0).WithMessage("被请求者材料ID必须大于0");

        RuleFor(x => x)
            .Must(x => x.RequesterMaterialId != x.ResponderMaterialId)
            .WithMessage("请求者材料ID和被请求者材料ID不能相同");
    }
}
