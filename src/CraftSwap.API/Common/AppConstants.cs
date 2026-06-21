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
}
