using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Data;

/// <summary>
/// 数据库初始化器
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public static void InitializeDatabase(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
            else
            {
                context.Database.EnsureCreated();
            }
        }
        catch (Exception)
        {
            context.Database.EnsureCreated();
        }
    }

    /// <summary>
    /// 异步初始化数据库
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <returns>表示异步操作的任务</returns>
    public static async Task InitializeDatabaseAsync(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }
        }
        catch (Exception)
        {
            await context.Database.EnsureCreatedAsync();
        }
    }
}
