using System.Text.RegularExpressions;
using CraftSwap.Common;

namespace CraftSwap.Services;

/// <summary>
/// 密码策略服务实现
/// </summary>
public class PasswordPolicyService : IPasswordPolicyService
{
    private static readonly char[] SpecialChars = "!@#$%^&*()_+-=[]{}|;':\",./<>?\\~`".ToCharArray();

    /// <summary>
    /// 校验密码是否符合安全策略
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>校验结果</returns>
    public PasswordValidationResult ValidatePassword(string password)
    {
        var result = new PasswordValidationResult();
        var errors = new List<string>();
        var tips = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("密码不能为空");
            result.IsValid = false;
            result.Errors = errors;
            result.Tips = GetPasswordRequirements();
            return result;
        }

        if (password.Length < AppConstants.PasswordPolicy.MinimumLength)
        {
            errors.Add($"密码长度不能少于 {AppConstants.PasswordPolicy.MinimumLength} 个字符");
        }

        if (password.Length > AppConstants.PasswordPolicy.MaximumLength)
        {
            errors.Add($"密码长度不能超过 {AppConstants.PasswordPolicy.MaximumLength} 个字符");
        }

        if (AppConstants.PasswordPolicy.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("密码必须包含至少一个大写字母");
        }

        if (AppConstants.PasswordPolicy.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("密码必须包含至少一个小写字母");
        }

        if (AppConstants.PasswordPolicy.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("密码必须包含至少一个数字");
        }

        if (AppConstants.PasswordPolicy.RequireNonAlphanumeric && !password.Any(c => SpecialChars.Contains(c)))
        {
            errors.Add("密码必须包含至少一个特殊字符（如 !@#$%^&* 等）");
        }

        if (IsCommonWeakPassword(password))
        {
            errors.Add("密码过于常见或简单，请使用更复杂的密码");
        }

        if (HasRepeatedCharacters(password))
        {
            tips.Add("建议避免使用连续重复的字符");
        }

        if (HasSequentialCharacters(password))
        {
            tips.Add("建议避免使用连续的字母或数字序列");
        }

        result.StrengthScore = CalculatePasswordStrength(password);
        result.StrengthDescription = GetStrengthDescription(result.StrengthScore);

        if (result.StrengthScore < AppConstants.PasswordPolicy.WeakPasswordScoreThreshold)
        {
            errors.Add($"密码强度不足（当前：{result.StrengthDescription}），请设置更复杂的密码");
        }

        result.IsValid = errors.Count == 0;
        result.Errors = errors;
        result.Tips = tips.Count > 0 ? tips : GetPasswordRequirements();

        return result;
    }

    /// <summary>
    /// 检查密码是否与历史密码重复
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="passwordHashes">历史密码哈希列表</param>
    /// <returns>是否重复</returns>
    public bool IsPasswordInHistory(string password, IEnumerable<string> passwordHashes)
    {
        if (string.IsNullOrWhiteSpace(password) || passwordHashes == null)
        {
            return false;
        }

        return passwordHashes.Any(hash =>
            !string.IsNullOrWhiteSpace(hash) &&
            BCrypt.Net.BCrypt.Verify(password, hash));
    }

    /// <summary>
    /// 计算密码强度评分
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>强度评分（1-5）</returns>
    public int CalculatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return 0;
        }

        int score = 0;

        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;
        if (password.Length >= 16) score++;

        if (password.Any(char.IsLower)) score++;
        if (password.Any(char.IsUpper)) score++;
        if (password.Any(char.IsDigit)) score++;
        if (password.Any(c => SpecialChars.Contains(c))) score++;

        int uniqueChars = password.Distinct().Count();
        if (uniqueChars >= 10) score++;
        if (uniqueChars >= 15) score++;

        if (HasRepeatedCharacters(password)) score--;
        if (HasSequentialCharacters(password)) score--;
        if (IsCommonWeakPassword(password)) score -= 2;

        score = Math.Max(1, Math.Min(5, score));

        return score;
    }

    /// <summary>
    /// 获取密码强度描述
    /// </summary>
    /// <param name="score">强度评分</param>
    /// <returns>强度描述</returns>
    public string GetStrengthDescription(int score)
    {
        return score switch
        {
            <= 1 => "非常弱",
            2 => "弱",
            3 => "中等",
            4 => "强",
            _ => "非常强"
        };
    }

    /// <summary>
    /// 生成密码建议提示
    /// </summary>
    /// <returns>提示列表</returns>
    public List<string> GetPasswordRequirements()
    {
        var requirements = new List<string>
        {
            $"密码长度为 {AppConstants.PasswordPolicy.MinimumLength}-{AppConstants.PasswordPolicy.MaximumLength} 个字符"
        };

        if (AppConstants.PasswordPolicy.RequireUppercase)
        {
            requirements.Add("包含至少一个大写字母");
        }

        if (AppConstants.PasswordPolicy.RequireLowercase)
        {
            requirements.Add("包含至少一个小写字母");
        }

        if (AppConstants.PasswordPolicy.RequireDigit)
        {
            requirements.Add("包含至少一个数字");
        }

        if (AppConstants.PasswordPolicy.RequireNonAlphanumeric)
        {
            requirements.Add("包含至少一个特殊字符（如 !@#$%^&* 等）");
        }

        requirements.Add("避免使用常见密码、连续字符或重复字符");

        return requirements;
    }

    /// <summary>
    /// 检查是否为常见弱密码
    /// </summary>
    private static bool IsCommonWeakPassword(string password)
    {
        var weakPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password", "password123", "password1",
            "123456", "12345678", "123456789", "1234567890",
            "qwerty", "qwerty123", "qwertyuiop",
            "abc123", "abcdef", "abcd1234",
            "111111", "11111111", "000000", "00000000",
            "admin", "admin123", "adminpassword",
            "letmein", "welcome", "monkey", "dragon",
            "master", "iloveyou", "trustno1",
            "sunshine", "princess", "football",
            "shadow", "654321", "superman"
        };

        var lowerPassword = password.ToLower();
        return weakPasswords.Contains(lowerPassword);
    }

    /// <summary>
    /// 检查是否有重复字符
    /// </summary>
    private static bool HasRepeatedCharacters(string password)
    {
        if (password.Length < 4) return false;

        int repeatCount = 1;
        for (int i = 1; i < password.Length; i++)
        {
            if (password[i] == password[i - 1])
            {
                repeatCount++;
                if (repeatCount >= 4) return true;
            }
            else
            {
                repeatCount = 1;
            }
        }

        return false;
    }

    /// <summary>
    /// 检查是否有连续字符
    /// </summary>
    private static bool HasSequentialCharacters(string password)
    {
        if (password.Length < 4) return false;

        int forwardCount = 1;
        int backwardCount = 1;

        for (int i = 1; i < password.Length; i++)
        {
            if (password[i] == password[i - 1] + 1)
            {
                forwardCount++;
                if (forwardCount >= 4) return true;
            }
            else
            {
                forwardCount = 1;
            }

            if (password[i] == password[i - 1] - 1)
            {
                backwardCount++;
                if (backwardCount >= 4) return true;
            }
            else
            {
                backwardCount = 1;
            }
        }

        return false;
    }
}
