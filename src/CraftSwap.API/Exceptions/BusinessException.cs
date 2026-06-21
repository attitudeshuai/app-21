namespace CraftSwap.Exceptions;

public class BusinessException : Exception
{
    public int ErrorCode { get; set; }

    public BusinessException(string message) : base(message)
    {
        ErrorCode = 400;
    }

    public BusinessException(int errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = 400;
    }
}
