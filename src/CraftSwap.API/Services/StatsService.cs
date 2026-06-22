using CraftSwap.Common;
using CraftSwap.DTOs.Common;
using CraftSwap.DTOs.Stats;
using CraftSwap.Repositories;
using Microsoft.Extensions.Caching.Memory;

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
    private readonly IMemoryCache _cache;

    /// <summary>
    /// 构造函数
    /// </summary>
    public StatsService(
        IUserRepository userRepository,
        IMaterialRepository materialRepository,
        ISwapRequestRepository swapRequestRepository,
        ISwapReviewRepository _swapReviewRepository,
        IProjectShowcaseRepository projectShowcaseRepository,
        IMemoryCache cache)
    {
        _userRepository = userRepository;
        _materialRepository = materialRepository;
        _swapRequestRepository = swapRequestRepository;
        this._swapReviewRepository = _swapReviewRepository;
        _projectShowcaseRepository = projectShowcaseRepository;
        _cache = cache;
    }

    /// <summary>
    /// 获取概览统计数据
    /// </summary>
    public async Task<ApiResponse<OverviewStatsResponse>> GetOverviewAsync()
    {
        var cacheKey = AppConstants.StatsCacheKeys.OverviewPrefix;
        if (_cache.TryGetValue(cacheKey, out OverviewStatsResponse? cachedResult) && cachedResult != null)
        {
            return ApiResponse<OverviewStatsResponse>.Success(cachedResult, "获取成功");
        }

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

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AppConstants.StatsCacheKeys.OverviewExpirationMinutes)
        };
        _cache.Set(cacheKey, overviewStats, cacheOptions);

        return ApiResponse<OverviewStatsResponse>.Success(overviewStats, "获取成功");
    }

    /// <summary>
    /// 获取趋势统计数据（旧版，按日粒度，向后兼容）
    /// </summary>
    public async Task<ApiResponse<TrendStatsResponse>> GetTrendAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            return ApiResponse<TrendStatsResponse>.Fail("开始日期不能晚于结束日期");
        }

        var parameters = new TrendQueryParameters
        {
            StartDate = startDate,
            EndDate = endDate,
            Granularity = TrendTimeGranularity.Day
        };

        var detailResult = await GetTrendDetailAsync(parameters);
        if (detailResult.Code != 200 || detailResult.Data == null)
        {
            return ApiResponse<TrendStatsResponse>.Fail(detailResult.Message);
        }

        var trendStats = new TrendStatsResponse
        {
            Dates = detailResult.Data.DataPoints.Select(d => d.TimeLabel).ToList(),
            UserGrowth = detailResult.Data.DataPoints.Select(d => d.NewUsers).ToList(),
            MaterialGrowth = detailResult.Data.DataPoints.Select(d => d.NewMaterials).ToList(),
            SwapRequestGrowth = detailResult.Data.DataPoints.Select(d => d.NewSwapRequests).ToList(),
            CompletedSwapGrowth = detailResult.Data.DataPoints.Select(d => d.CompletedSwaps).ToList()
        };

        return ApiResponse<TrendStatsResponse>.Success(trendStats, "获取成功");
    }

    /// <summary>
    /// 获取细粒度趋势统计数据
    /// </summary>
    public async Task<ApiResponse<TrendStatsDetailResponse>> GetTrendDetailAsync(TrendQueryParameters parameters)
    {
        var validation = ValidateParameters(parameters);
        if (!validation.IsValid)
        {
            return ApiResponse<TrendStatsDetailResponse>.Fail(validation.ErrorMessage!);
        }

        parameters = NormalizeParameters(parameters);

        var cacheKey = $"{AppConstants.StatsCacheKeys.TrendPrefix}:{parameters.Granularity}:{parameters.StartDate:yyyyMMdd}:{parameters.EndDate:yyyyMMdd}";
        if (_cache.TryGetValue(cacheKey, out TrendStatsDetailResponse? cachedResult) && cachedResult != null)
        {
            return ApiResponse<TrendStatsDetailResponse>.Success(cachedResult, "获取成功");
        }

        var actualStartDate = GetPeriodStart(parameters.StartDate, parameters.Granularity);
        var actualEndDate = GetPeriodEnd(parameters.EndDate, parameters.Granularity);

        var materialDailyCounts = await _materialRepository.GetDailyCountGroupedAsync(actualStartDate, actualEndDate.AddDays(1));
        var swapRequestDailyCounts = await _swapRequestRepository.GetDailyCountGroupedAsync(actualStartDate, actualEndDate.AddDays(1));
        var completedSwapDailyCounts = await _swapRequestRepository.GetDailyCountByStatusGroupedAsync(
            AppConstants.SwapRequestStatus.Completed, actualStartDate, actualEndDate.AddDays(1));
        var userDailyCounts = await _userRepository.GetDailyCountGroupedAsync(actualStartDate, actualEndDate.AddDays(1));

        var dataPoints = BuildDataPoints(
            actualStartDate,
            actualEndDate,
            parameters.Granularity,
            materialDailyCounts,
            swapRequestDailyCounts,
            completedSwapDailyCounts,
            userDailyCounts);

        var totalDays = Math.Max(1, (int)(actualEndDate.Date - actualStartDate.Date).TotalDays + 1);
        var summary = BuildSummary(dataPoints, totalDays);

        var result = new TrendStatsDetailResponse
        {
            Granularity = parameters.Granularity,
            ActualStartDate = actualStartDate,
            ActualEndDate = actualEndDate,
            TotalPoints = dataPoints.Count,
            DataPoints = dataPoints,
            Summary = summary
        };

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AppConstants.StatsCacheKeys.TrendExpirationMinutes)
        };
        _cache.Set(cacheKey, result, cacheOptions);

        return ApiResponse<TrendStatsDetailResponse>.Success(result, "获取成功");
    }

    /// <summary>
    /// 验证查询参数
    /// </summary>
    private static (bool IsValid, string? ErrorMessage) ValidateParameters(TrendQueryParameters parameters)
    {
        if (parameters.StartDate > parameters.EndDate)
        {
            return (false, "开始日期不能晚于结束日期");
        }

        var totalDays = (parameters.EndDate.Date - parameters.StartDate.Date).TotalDays + 1;

        switch (parameters.Granularity)
        {
            case TrendTimeGranularity.Day:
                if (totalDays > AppConstants.StatsDefaults.MaxDailyQueryDays)
                {
                    return (false, $"按日查询最多支持 {AppConstants.StatsDefaults.MaxDailyQueryDays} 天");
                }
                break;
            case TrendTimeGranularity.Week:
                var totalWeeks = Math.Ceiling(totalDays / 7);
                if (totalWeeks > AppConstants.StatsDefaults.MaxWeeklyQueryWeeks)
                {
                    return (false, $"按周查询最多支持 {AppConstants.StatsDefaults.MaxWeeklyQueryWeeks} 周");
                }
                break;
            case TrendTimeGranularity.Month:
                var totalMonths = (parameters.EndDate.Year - parameters.StartDate.Year) * 12
                                + (parameters.EndDate.Month - parameters.StartDate.Month) + 1;
                if (totalMonths > AppConstants.StatsDefaults.MaxMonthlyQueryMonths)
                {
                    return (false, $"按月查询最多支持 {AppConstants.StatsDefaults.MaxMonthlyQueryMonths} 个月");
                }
                break;
            default:
                return (false, "不支持的时间粒度");
        }

        return (true, null);
    }

    /// <summary>
    /// 规范化参数，设置默认值
    /// </summary>
    private static TrendQueryParameters NormalizeParameters(TrendQueryParameters parameters)
    {
        var now = DateTime.UtcNow.Date;

        if (parameters.StartDate == default)
        {
            parameters.StartDate = now.AddDays(-(AppConstants.StatsDefaults.DefaultQueryDays - 1));
        }

        if (parameters.EndDate == default)
        {
            parameters.EndDate = now;
        }

        parameters.StartDate = parameters.StartDate.Date;
        parameters.EndDate = parameters.EndDate.Date;

        if (parameters.EndDate > now)
        {
            parameters.EndDate = now;
        }

        return parameters;
    }

    /// <summary>
    /// 获取周期起始日期
    /// </summary>
    private static DateTime GetPeriodStart(DateTime date, TrendTimeGranularity granularity)
    {
        return granularity switch
        {
            TrendTimeGranularity.Day => date.Date,
            TrendTimeGranularity.Week => GetWeekStart(date.Date),
            TrendTimeGranularity.Month => new DateTime(date.Year, date.Month, 1),
            _ => date.Date
        };
    }

    /// <summary>
    /// 获取周期结束日期
    /// </summary>
    private static DateTime GetPeriodEnd(DateTime date, TrendTimeGranularity granularity)
    {
        return granularity switch
        {
            TrendTimeGranularity.Day => date.Date,
            TrendTimeGranularity.Week => GetWeekEnd(date.Date),
            TrendTimeGranularity.Month => new DateTime(date.Year, date.Month,
                DateTime.DaysInMonth(date.Year, date.Month)),
            _ => date.Date
        };
    }

    /// <summary>
    /// 获取周起始（周一为一周开始）
    /// </summary>
    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// 获取周结束（周日为一周结束）
    /// </summary>
    private static DateTime GetWeekEnd(DateTime date)
    {
        return GetWeekStart(date).AddDays(6).Date;
    }

    /// <summary>
    /// 构建数据点
    /// </summary>
    private static List<TrendDataPoint> BuildDataPoints(
        DateTime actualStartDate,
        DateTime actualEndDate,
        TrendTimeGranularity granularity,
        Dictionary<DateTime, int> materialDailyCounts,
        Dictionary<DateTime, int> swapRequestDailyCounts,
        Dictionary<DateTime, int> completedSwapDailyCounts,
        Dictionary<DateTime, int> userDailyCounts)
    {
        var dataPoints = new List<TrendDataPoint>();
        var current = actualStartDate.Date;

        while (current <= actualEndDate.Date)
        {
            var (periodStart, periodEnd, timeLabel) = GetPeriodInfo(current, granularity);

            var newMaterials = SumDailyCounts(materialDailyCounts, periodStart, periodEnd);
            var newSwapRequests = SumDailyCounts(swapRequestDailyCounts, periodStart, periodEnd);
            var completedSwaps = SumDailyCounts(completedSwapDailyCounts, periodStart, periodEnd);
            var newUsers = SumDailyCounts(userDailyCounts, periodStart, periodEnd);

            dataPoints.Add(new TrendDataPoint
            {
                TimeLabel = timeLabel,
                NewMaterials = newMaterials,
                NewSwapRequests = newSwapRequests,
                CompletedSwaps = completedSwaps,
                NewUsers = newUsers
            });

            current = GetNextPeriod(current, granularity);
        }

        return dataPoints;
    }

    /// <summary>
    /// 获取周期信息
    /// </summary>
    private static (DateTime PeriodStart, DateTime PeriodEnd, string TimeLabel) GetPeriodInfo(
        DateTime current, TrendTimeGranularity granularity)
    {
        return granularity switch
        {
            TrendTimeGranularity.Day => (
                current.Date,
                current.Date,
                current.ToString("yyyy-MM-dd")),
            TrendTimeGranularity.Week => (
                GetWeekStart(current.Date),
                GetWeekEnd(current.Date),
                $"{GetWeekStart(current.Date):yyyy-MM-dd}~{GetWeekEnd(current.Date):yyyy-MM-dd}"),
            TrendTimeGranularity.Month => (
                new DateTime(current.Year, current.Month, 1),
                new DateTime(current.Year, current.Month,
                    DateTime.DaysInMonth(current.Year, current.Month)),
                current.ToString("yyyy-MM")),
            _ => (current.Date, current.Date, current.ToString("yyyy-MM-dd"))
        };
    }

    /// <summary>
    /// 获取下一周期起始日期
    /// </summary>
    private static DateTime GetNextPeriod(DateTime current, TrendTimeGranularity granularity)
    {
        return granularity switch
        {
            TrendTimeGranularity.Day => current.AddDays(1).Date,
            TrendTimeGranularity.Week => current.AddDays(7).Date,
            TrendTimeGranularity.Month => current.AddMonths(1).Date,
            _ => current.AddDays(1).Date
        };
    }

    /// <summary>
    /// 汇总日度数据到周期
    /// </summary>
    private static int SumDailyCounts(
        Dictionary<DateTime, int> dailyCounts,
        DateTime periodStart,
        DateTime periodEnd)
    {
        var sum = 0;
        var current = periodStart.Date;
        while (current <= periodEnd.Date)
        {
            if (dailyCounts.TryGetValue(current, out var count))
            {
                sum += count;
            }
            current = current.AddDays(1);
        }
        return sum;
    }

    /// <summary>
    /// 构建汇总统计
    /// </summary>
    private static TrendSummary BuildSummary(List<TrendDataPoint> dataPoints, int totalDays)
    {
        var summary = new TrendSummary
        {
            TotalNewMaterials = dataPoints.Sum(d => d.NewMaterials),
            TotalNewSwapRequests = dataPoints.Sum(d => d.NewSwapRequests),
            TotalCompletedSwaps = dataPoints.Sum(d => d.CompletedSwaps),
            TotalNewUsers = dataPoints.Sum(d => d.NewUsers)
        };

        summary.AvgNewMaterialsPerDay = Math.Round((double)summary.TotalNewMaterials / totalDays, 2);
        summary.AvgNewSwapRequestsPerDay = Math.Round((double)summary.TotalNewSwapRequests / totalDays, 2);
        summary.AvgCompletedSwapsPerDay = Math.Round((double)summary.TotalCompletedSwaps / totalDays, 2);
        summary.AvgNewUsersPerDay = Math.Round((double)summary.TotalNewUsers / totalDays, 2);

        return summary;
    }
}
