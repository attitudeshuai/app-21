using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftSwap.Entities;

/// <summary>
/// 系统日志实体
/// </summary>
[Table("SystemLogs")]
public class SystemLog
{
    /// <summary>
    /// 日志主键ID
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string LogLevel { get; set; } = string.Empty;

    /// <summary>
    /// 事件类型
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// 操作人用户ID
    /// </summary>
    public int? OperatorId { get; set; }

    /// <summary>
    /// 操作人用户名
    /// </summary>
    [MaxLength(50)]
    public string? OperatorName { get; set; }

    /// <summary>
    /// 目标用户ID（如果有）
    /// </summary>
    public int? TargetUserId { get; set; }

    /// <summary>
    /// 日志消息
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 详细信息（JSON格式）
    /// </summary>
    [MaxLength(4000)]
    public string? Details { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 操作人导航属性
    /// </summary>
    [ForeignKey(nameof(OperatorId))]
    public User? Operator { get; set; }

    /// <summary>
    /// 目标用户导航属性
    /// </summary>
    [ForeignKey(nameof(TargetUserId))]
    public User? TargetUser { get; set; }
}
