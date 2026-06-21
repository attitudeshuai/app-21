using CraftSwap.DTOs.Auth;
using FluentValidation;

namespace CraftSwap.Validators;

/// <summary>
/// 刷新令牌请求验证器
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    /// <summary>
    /// 构造函数，配置验证规则
    /// </summary>
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("访问令牌不能为空");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("刷新令牌不能为空");
    }
}
