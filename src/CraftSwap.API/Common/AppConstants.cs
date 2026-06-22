namespace CraftSwap.Common;

/// <summary>
/// 应用常量
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// 默认页码
    /// </summary>
    public const int DefaultPageNumber = 1;

    /// <summary>
    /// 默认每页大小
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// 最大每页大小
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// 默认排序方向
    /// </summary>
    public const string DefaultSortDirection = "desc";

    /// <summary>
    /// 材料状态
    /// </summary>
    public static class MaterialStatus
    {
        /// <summary>
        /// 可用
        /// </summary>
        public const string Available = "Available";

        /// <summary>
        /// 已交换
        /// </summary>
        public const string Swapped = "Swapped";

        /// <summary>
        /// 已下架
        /// </summary>
        public const string Offline = "Offline";

        /// <summary>
        /// 审核中
        /// </summary>
        public const string Pending = "Pending";
    }

    /// <summary>
    /// 交换请求状态
    /// </summary>
    public static class SwapRequestStatus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        public const string Pending = "Pending";

        /// <summary>
        /// 已接受
        /// </summary>
        public const string Accepted = "Accepted";

        /// <summary>
        /// 已拒绝
        /// </summary>
        public const string Rejected = "Rejected";

        /// <summary>
        /// 已取消
        /// </summary>
        public const string Cancelled = "Cancelled";

        /// <summary>
        /// 进行中
        /// </summary>
        public const string InProgress = "InProgress";

        /// <summary>
        /// 已完成
        /// </summary>
        public const string Completed = "Completed";
    }

    /// <summary>
    /// 用户角色
    /// </summary>
    public static class UserRoles
    {
        /// <summary>
        /// 管理员
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// 普通用户
        /// </summary>
        public const string User = "User";
    }

    /// <summary>
    /// 系统日志级别
    /// </summary>
    public static class LogLevels
    {
        /// <summary>
        /// 信息
        /// </summary>
        public const string Information = "Information";

        /// <summary>
        /// 警告
        /// </summary>
        public const string Warning = "Warning";

        /// <summary>
        /// 错误
        /// </summary>
        public const string Error = "Error";

        /// <summary>
        /// 关键
        /// </summary>
        public const string Critical = "Critical";
    }

    /// <summary>
    /// 统计缓存键
    /// </summary>
    public static class StatsCacheKeys
    {
        /// <summary>
        /// 概览统计缓存键前缀
        /// </summary>
        public const string OverviewPrefix = "stats:overview";

        /// <summary>
        /// 趋势统计缓存键前缀
        /// </summary>
        public const string TrendPrefix = "stats:trend";

        /// <summary>
        /// 概览统计缓存过期时间（分钟）
        /// </summary>
        public const int OverviewExpirationMinutes = 5;

        /// <summary>
        /// 趋势统计缓存过期时间（分钟）
        /// </summary>
        public const int TrendExpirationMinutes = 10;
    }

    /// <summary>
    /// 统计默认值
    /// </summary>
    public static class StatsDefaults
    {
        /// <summary>
        /// 默认查询天数
        /// </summary>
        public const int DefaultQueryDays = 7;

        /// <summary>
        /// 最大查询天数（按日粒度）
        /// </summary>
        public const int MaxDailyQueryDays = 365;

        /// <summary>
        /// 最大查询周数（按周粒度）
        /// </summary>
        public const int MaxWeeklyQueryWeeks = 104;

        /// <summary>
        /// 最大查询月数（按月粒度）
        /// </summary>
        public const int MaxMonthlyQueryMonths = 36;
    }

    /// <summary>
    /// 密码安全策略
    /// </summary>
    public static class PasswordPolicy
    {
        /// <summary>
        /// 密码最小长度
        /// </summary>
        public const int MinimumLength = 8;

        /// <summary>
        /// 密码最大长度
        /// </summary>
        public const int MaximumLength = 100;

        /// <summary>
        /// 是否要求包含大写字母
        /// </summary>
        public const bool RequireUppercase = true;

        /// <summary>
        /// 是否要求包含小写字母
        /// </summary>
        public const bool RequireLowercase = true;

        /// <summary>
        /// 是否要求包含数字
        /// </summary>
        public const bool RequireDigit = true;

        /// <summary>
        /// 是否要求包含特殊字符
        /// </summary>
        public const bool RequireNonAlphanumeric = true;

        /// <summary>
        /// 历史密码检查数量（不能与最近N次密码重复）
        /// </summary>
        public const int HistoryCheckCount = 5;

        /// <summary>
        /// 密码过期天数（0表示不过期）
        /// </summary>
        public const int PasswordExpiryDays = 90;

        /// <summary>
        /// 弱密码检测阈值（密码强度得分低于此值视为弱密码）
        /// </summary>
        public const int WeakPasswordScoreThreshold = 3;
    }

    /// <summary>
    /// 系统事件类型
    /// </summary>
    public static class EventTypes
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        public const string UserLogin = "UserLogin";

        /// <summary>
        /// 用户登录失败
        /// </summary>
        public const string UserLoginFailed = "UserLoginFailed";

        /// <summary>
        /// 用户注册
        /// </summary>
        public const string UserRegistered = "UserRegistered";

        /// <summary>
        /// 用户资料更新
        /// </summary>
        public const string UserProfileUpdated = "UserProfileUpdated";

        /// <summary>
        /// 用户被锁定
        /// </summary>
        public const string UserLocked = "UserLocked";

        /// <summary>
        /// 用户被解锁
        /// </summary>
        public const string UserUnlocked = "UserUnlocked";

        /// <summary>
        /// 用户列表查询
        /// </summary>
        public const string UserListQueried = "UserListQueried";

        /// <summary>
        /// 系统日志查询
        /// </summary>
        public const string SystemLogQueried = "SystemLogQueried";

        /// <summary>
        /// 权限不足
        /// </summary>
        public const string PermissionDenied = "PermissionDenied";
    }
}
