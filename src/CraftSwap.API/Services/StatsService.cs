using CraftSwap.Common;
using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.Stats;
using CraftSwap.Repositories;

namespace CraftSwap.Services;

/// <summary>
/// 统计服务实现
/// </summary>
public class StatsService : IStatsService
{
    private readonly IUserRepository _userRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly ISwapRequestRepository _swapRequestRepository;
    private readonly ISwapReviewRepository _swapReviewRepository;
    private readonly IProjectShowcaseRepository _projectShowcaseRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="materialRepository">材料仓储</param>
    /// <param name="swapRequestRepository">交换请求仓储</param>
    /// <param name="swapReviewRepository">评价仓储</param>
    /// <param name="projectShowcaseRepository">作品仓储</param>
    public StatsService(
        IUserRepository userRepository,
        IMaterialRepository materialRepository,
        ISwapRequestRepository swapRequestRepository,
        ISwapReviewRepository swapReviewRepository,
        IProjectShowcaseRepository projectShowcaseRepository)
    {
        _userRepository = userRepository;
        _materialRepository = materialRepository;
        _swapRequestRepository = swapRequestRepository;
        _swapReviewRepository = swapReviewRepository;
        _projectShowcaseRepository = projectShowcaseRepository;
    }

    /// <summary>
    /// 获取概览统计数据
    /// </summary>
    /// <returns>概览统计响应</returns>
    public async Task<ApiResponse<OverviewStatsResponse>> GetOverviewAsync()
    {
        var today = DateTime.UtcNow.Date;

        var totalUsers = await _userRepository.CountAsync();
        var totalMaterials = await _materialRepository.CountAsync();
        var totalSwapRequests = await _swapRequestRepository.CountAsync();
        var completedSwaps = await _swapRequestRepository.GetCountByStatusAsync(AppConstants.SwapRequestStatus.Completed);
        var totalProjectShowcases = await _projectShowcaseRepository.CountAsync();
        var totalReviews = await _swapReviewRepository.CountAsync();

        var todayNewUsers = await _userRepository.GetCountByDateRangeAsync(today, today.AddDays(1));
        var todayNewMaterials = await _materialRepository.GetCountByDateRangeAsync(today, today.AddDays(1));
        var todayNewSwapRequests = await _swapRequestRepository.GetCountByDateRangeAsync(today, today.AddDays(1));

        var overviewStats = new OverviewStatsResponse
        {
            TotalUsers = totalUsers,
            TotalMaterials = totalMaterials,
            TotalSwapRequests = totalSwapRequests,
            CompletedSwaps = completedSwaps,
            TotalProjectShowcases = totalProjectShowcases,
            TotalReviews = totalReviews,
            TodayNewUsers = todayNewUsers,
            TodayNewMaterials = todayNewMaterials,
            TodayNewSwapRequests = todayNewSwapRequests
        };

        return ApiResponse<OverviewStatsResponse>.Success(overviewStats, "获取成功");
    }

    /// <summary>
    /// 获取趋势统计数据
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>趋势统计响应</returns>
    public async Task<ApiResponse<TrendStatsResponse>> GetTrendAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            return ApiResponse<TrendStatsResponse>.Fail("开始日期不能晚于结束日期");
        }

        var dates = new List<string>();
        var userGrowth = new List<int>();
        var materialGrowth = new List<int>();
        var swapRequestGrowth = new List<int>();
        var completedSwapGrowth = new List<int>();

        var currentDate = startDate.Date;
        var endDateOnly = endDate.Date;

        while (currentDate <= endDateOnly)
        {
            var nextDate = currentDate.AddDays(1);
            dates.Add(currentDate.ToString("yyyy-MM-dd"));

            var dayUserCount = await _userRepository.GetCountByDateRangeAsync(currentDate, nextDate);
            var dayMaterialCount = await _materialRepository.GetCountByDateRangeAsync(currentDate, nextDate);
            var daySwapRequestCount = await _swapRequestRepository.GetCountByDateRangeAsync(currentDate, nextDate);
            var dayCompletedSwapCount = await _swapRequestRepository.GetCountByStatusAndDateRangeAsync(
                AppConstants.SwapRequestStatus.Completed, currentDate, nextDate);

            userGrowth.Add(dayUserCount);
            materialGrowth.Add(dayMaterialCount);
            swapRequestGrowth.Add(daySwapRequestCount);
            completedSwapGrowth.Add(dayCompletedSwapCount);

            currentDate = currentDate.AddDays(1);
        }

        var trendStats = new TrendStatsResponse
        {
            Dates = dates,
            UserGrowth = userGrowth,
            MaterialGrowth = materialGrowth,
            SwapRequestGrowth = swapRequestGrowth,
            CompletedSwapGrowth = completedSwapGrowth
        };

        return ApiResponse<TrendStatsResponse>.Success(trendStats, "获取成功");
    }
}
