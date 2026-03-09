namespace Inventory_Management_Platform.Common;

public sealed record ApiResponse<T>(bool Success, int Status, T? Data);

public sealed record ApiErrorResponse(bool Success, int Status, string Message, string ErrorCode);

public static class ApiResponse
{
  public static ApiResponse<T> Ok<T>(T data, int status = 200) =>
      new(true, status, data);

  public static ApiErrorResponse Fail(int status, string message, string errorCode = "error") =>
      new(false, status, message, errorCode);
}
