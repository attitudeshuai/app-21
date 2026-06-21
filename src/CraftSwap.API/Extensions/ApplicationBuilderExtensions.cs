using CraftSwap.Data;
using CraftSwap.Middleware;
using Microsoft.EntityFrameworkCore;

namespace CraftSwap.Extensions;

/// <summary>
/// 应用程序构建器扩展方法类
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 使用自定义异常处理中间件
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }

    /// <summary>
    /// 配置Swagger UI
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "CraftSwap API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "CraftSwap API 文档";
            options.DefaultModelsExpandDepth(-1);
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.ShowExtensions();
        });
        return app;
    }

    /// <summary>
    /// 初始化数据库（自动迁移和种子数据）
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseDatabaseInitializer(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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

        return app;
    }

    /// <summary>
    /// 异步初始化数据库（自动迁移和种子数据）
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>表示异步操作的任务</returns>
    public static async Task<IApplicationBuilder> UseDatabaseInitializerAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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

        return app;
    }
}
