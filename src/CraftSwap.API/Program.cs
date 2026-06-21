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
    
    var retryCount = 0;
    var maxRetries = 10;
    while (retryCount < maxRetries)
    {
        try
        {
            retryCount++;
            logger.LogInformation($"正在初始化数据库，尝试 {retryCount}/{maxRetries}...");
            
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
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
            logger.LogError(ex, $"数据库初始化失败（尝试 {retryCount}/{maxRetries}），5秒后重试...");
            if (retryCount >= maxRetries)
            {
                logger.LogCritical(ex, "数据库初始化最终失败，但应用仍将尝试启动...");
            }
            await Task.Delay(5000);
        }
    }
}

app.Run();
