using System.Text.Json;
using CraftSwap.DTOs.Common;
using CraftSwap.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CraftSwap.Middleware;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="next">请求委托</param>
    /// <param name="logger">日志记录器</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 执行中间件逻辑
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "参数验证异常发生: {Message}", ex.Message);
            var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
            var errorMessage = errors.Count > 0 ? string.Join("; ", errors) : ex.Message;
            await WriteResponseAsync(context, StatusCodes.Status400BadRequest, ApiResponse.Fail(errorMessage, 400));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "未授权访问异常发生: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status401Unauthorized, ApiResponse.Fail(string.IsNullOrEmpty(ex.Message) ? "未授权访问" : ex.Message, 401));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "资源未找到异常发生: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status404NotFound, ApiResponse.Fail(string.IsNullOrEmpty(ex.Message) ? "资源未找到" : ex.Message, 404));
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "业务异常发生: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status400BadRequest, ApiResponse.Fail(ex.Message, ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "系统异常发生: {Message}", ex.Message);
            await WriteResponseAsync(context, StatusCodes.Status500InternalServerError, ApiResponse.Fail("服务器内部错误", 500));
        }
    }

    /// <summary>
    /// 写入统一格式的响应
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="statusCode">HTTP状态码</param>
    /// <param name="response">统一响应对象</param>
    /// <returns>表示异步操作的任务</returns>
    private static Task WriteResponseAsync(HttpContext context, int statusCode, ApiResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
