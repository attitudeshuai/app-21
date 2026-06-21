using CraftSwap.Data;
using CraftSwap.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCustomExceptionHandler();

app.UseSwaggerUI();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();
    
    var supportsMigrations = context.Database.IsRelational();

    var retryCount = 0;
    var maxRetries = supportsMigrations ? 10 : 1;
    var retryDelay = supportsMigrations ? 5000 : 0;
    while (retryCount < maxRetries)
    {
        try
        {
            retryCount++;
            logger.LogInformation($"正在初始化数据库，尝试 {retryCount}/{maxRetries}...");

            if (supportsMigrations)
            {
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();
                }
                else
                {
                    await context.Database.EnsureCreatedAsync();
                }
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }
            
            logger.LogInformation("数据库初始化成功！");
            break;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"数据库初始化失败（尝试 {retryCount}/{maxRetries}）{(retryDelay > 0 ? $", {retryDelay / 1000}秒后重试" : string.Empty)}...");
            if (retryCount >= maxRetries)
            {
                logger.LogCritical(ex, "数据库初始化最终失败，但应用仍将尝试启动...");
            }
            if (retryDelay > 0)
            {
                await Task.Delay(retryDelay);
            }
        }
    }
}

app.Run();

public partial class Program { }
