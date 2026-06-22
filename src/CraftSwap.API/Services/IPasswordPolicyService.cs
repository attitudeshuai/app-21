namespace CraftSwap.Services;

/// <summary>
/// 密码校验结果
/// </summary>
public class PasswordValidationResult
{
    /// <summary>
    /// 是否通过校验
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 密码强度评分（1-5分）
    /// </summary>
    public int StrengthScore { get; set; }

    /// <summary>
    /// 密码强度描述
    /// </summary>
    public string StrengthDescription { get; set; } = string.Empty;

    /// <summary>
    /// 提示信息（用于友好提示）
    /// </summary>
    public List<string> Tips { get; set; } = new();
}

/// <summary>
/// 密码策略服务接口
/// </summary>
public interface IPasswordPolicyService
{
    /// <summary>
    /// 校验密码是否符合安全策略
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>校验结果</returns>
    PasswordValidationResult ValidatePassword(string password);

    /// <summary>
    /// 检查密码是否与历史密码重复
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="passwordHashes">历史密码哈希列表</param>
    /// <returns>是否重复</returns>
    bool IsPasswordInHistory(string password, IEnumerable<string> passwordHashes);

    /// <summary>
    /// 计算密码强度评分
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>强度评分（1-5）</returns>
    int CalculatePasswordStrength(string password);

    /// <summary>
    /// 获取密码强度描述
    /// </summary>
    /// <param name="score">强度评分</param>
    /// <returns>强度描述</returns>
    string GetStrengthDescription(int score);

    /// <summary>
    /// 生成密码建议提示
    /// </summary>
    /// <returns>提示列表</returns>
    List<string> GetPasswordRequirements();
}
