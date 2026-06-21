using CraftSwap.DTOs.Auth;
using CraftSwap.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CraftSwap.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    private static RegisterRequest CreateValidRequest()
    {
        return new RegisterRequest
        {
            Username = "validuser",
            Email = "valid@test.com",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            Nickname = "Valid User",
            AvatarUrl = "https://example.com/avatar.jpg"
        };
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Username = string.Empty;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("用户名不能为空");
    }

    [Fact]
    public void Validate_WhenUsernameIsTooShort_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Username = "ab";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("用户名长度必须在3到50个字符之间");
    }

    [Fact]
    public void Validate_WhenUsernameIsTooLong_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Username = new string('a', 51);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("用户名长度必须在3到50个字符之间");
    }

    [Fact]
    public void Validate_WhenUsernameContainsSpecialCharacters_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Username = "user@name";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("用户名只能包含字母、数字和下划线");
    }

    [Fact]
    public void Validate_WhenUsernameContainsChineseCharacters_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Username = "用户123";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("用户名只能包含字母、数字和下划线");
    }

    [Fact]
    public void Validate_WhenUsernameIsValid_ShouldNotHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Username = "user_name123";

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Email = string.Empty;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("邮箱不能为空");
    }

    [Fact]
    public void Validate_WhenEmailFormatIsInvalid_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Email = "invalid-email";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("邮箱格式不正确");
    }

    [Fact]
    public void Validate_WhenEmailIsTooLong_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Email = new string('a', 93) + "@test.com";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("邮箱长度不能超过100个字符");
    }

    [Fact]
    public void Validate_WhenEmailIsValid_ShouldNotHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Email = "test.user+tag@example-domain.co.uk";

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Password = string.Empty;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("密码不能为空");
    }

    [Fact]
    public void Validate_WhenPasswordIsTooShort_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Password = "Ab1";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("密码长度必须在6到100个字符之间");
    }

    [Fact]
    public void Validate_WhenPasswordHasOnlyLetters_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Password = "OnlyLetters";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("密码必须包含字母和数字");
    }

    [Fact]
    public void Validate_WhenPasswordHasOnlyDigits_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Password = "12345678";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("密码必须包含字母和数字");
    }

    [Fact]
    public void Validate_WhenPasswordIsValid_ShouldNotHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Password = "ValidPwd123";

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WhenConfirmPasswordIsEmpty_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.ConfirmPassword = string.Empty;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("确认密码不能为空");
    }

    [Fact]
    public void Validate_WhenConfirmPasswordDoesNotMatch_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Password = "Test@123456";
        request.ConfirmPassword = "Different@123";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("两次输入的密码不一致");
    }

    [Fact]
    public void Validate_WhenConfirmPasswordMatches_ShouldNotHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Password = "Test@123456";
        request.ConfirmPassword = "Test@123456";

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void Validate_WhenNicknameIsTooLong_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Nickname = new string('x', 51);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage("昵称长度不能超过50个字符");
    }

    [Fact]
    public void Validate_WhenNicknameIsValid_ShouldNotHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Nickname = "我是一个合法的昵称";

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Nickname);
    }

    [Fact]
    public void Validate_WhenNicknameIsNull_ShouldNotHaveValidationError()
    {
        var request = CreateValidRequest();
        request.Nickname = null;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Nickname);
    }

    [Fact]
    public void Validate_WhenAvatarUrlIsTooLong_ShouldHaveValidationError()
    {
        var request = CreateValidRequest();
        request.AvatarUrl = "https://example.com/" + new string('x', 490);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.AvatarUrl)
            .WithErrorMessage("头像URL长度不能超过500个字符");
    }

    [Fact]
    public void Validate_WhenAvatarUrlIsEmpty_ShouldNotHaveValidationError()
    {
        var request = CreateValidRequest();
        request.AvatarUrl = string.Empty;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.AvatarUrl);
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldPassValidation()
    {
        var request = CreateValidRequest();

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenAllFieldsInvalid_ShouldHaveMultipleErrors()
    {
        var request = new RegisterRequest
        {
            Username = "a@",
            Email = "not-an-email",
            Password = "abc",
            ConfirmPassword = "xyz",
            Nickname = new string('x', 100),
            AvatarUrl = "https://example.com/" + new string('x', 600)
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(3);
    }

    [Fact]
    public void Validate_WhenUsernameHasUnderscores_ShouldBeValid()
    {
        var request = CreateValidRequest();
        request.Username = "user_name_test_123";

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_WhenPasswordIsExactly6Chars_ShouldBeValidIfHasLetterAndDigit()
    {
        var request = CreateValidRequest();
        request.Password = "Ab1234";
        request.ConfirmPassword = "Ab1234";

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WhenEmailMissingAtSign_ShouldBeInvalid()
    {
        var request = CreateValidRequest();
        request.Email = "test.test.com";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("邮箱格式不正确");
    }
}
