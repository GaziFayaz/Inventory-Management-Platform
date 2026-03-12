namespace Inventory_Management_Platform.Common.Errors;

public class AppException(int statusCode, string message, string errorCode = ErrorCodes.Fallback)
    : Exception(message)
{
  public int StatusCode { get; } = statusCode;
  public string ErrorCode { get; } = errorCode;
}
