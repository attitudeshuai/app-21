namespace CraftSwap.DTOs.Common;

/// <summary>
/// 统一API返回格式
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 数据
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse Success(object? data = null, string message = "操作成功")
    {
        return new ApiResponse { Code = 200, Message = message, Data = data };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static ApiResponse Fail(string message = "操作失败", int code = 400)
    {
        return new ApiResponse { Code = code, Message = message, Data = null };
    }
}

/// <summary>
/// 泛型统一API返回格式
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse<T> Success(T? data, string message = "操作成功")
    {
        return new ApiResponse<T> { Code = 200, Message = message, Data = data };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static ApiResponse<T> Fail(string message = "操作失败", int code = 400)
    {
        return new ApiResponse<T> { Code = code, Message = message, Data = default };
    }
}
