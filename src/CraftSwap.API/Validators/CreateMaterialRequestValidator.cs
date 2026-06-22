using CraftSwap.DTOs.Materials;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 创建材料请求验证器
/// </summary>
public class CreateMaterialRequestValidator : AbstractValidator<CreateMaterialRequest>
{
    /// <summary>
    /// 构造函数，配置创建材料请求验证规则
    /// </summary>
    public CreateMaterialRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("标题不能为空")
            .Length(2, 100).WithMessage("标题长度必须在2到100个字符之间");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("描述长度不能超过2000个字符")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("分类不能为空")
            .MaximumLength(50).WithMessage("分类长度不能超过50个字符");

        RuleFor(x => x.ImageUrls)
            .Must(x => x == null || x.Count <= 20).WithMessage("图片数量不能超过20张");

        RuleForEach(x => x.ImageUrls)
            .NotEmpty().WithMessage("图片URL不能为空")
            .MaximumLength(500).WithMessage("图片URL长度不能超过500个字符")
            .When(x => x.ImageUrls != null && x.ImageUrls.Count > 0);

        RuleFor(x => x.Tags)
            .Must(x => x == null || x.Count <= 10).WithMessage("标签数量不能超过10个");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("标签内容不能为空")
            .MaximumLength(30).WithMessage("单个标签长度不能超过30个字符")
            .When(x => x.Tags != null && x.Tags.Count > 0);

        RuleFor(x => x.Tags)
            .Must(x => x == null || string.Join(',', x).Length <= 500).WithMessage("标签总长度不能超过500个字符")
            .When(x => x.Tags != null && x.Tags.Count > 0);
    }
}
