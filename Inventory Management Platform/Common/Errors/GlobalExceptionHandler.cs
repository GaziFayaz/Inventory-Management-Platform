using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_Platform.Common.Errors;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment env) : IExceptionHandler
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public async ValueTask<bool> TryHandleAsync(
      HttpContext context,
      Exception exception,
      CancellationToken cancellationToken)
  {
    if (exception is OperationCanceledException)
      return true;

    var (status, message, errorCode) = exception switch
    {
      AppException ex => (ex.StatusCode, ex.Message, ex.ErrorCode),
      DbUpdateConcurrencyException => (409, "A conflicting update occurred. Please retry.", ErrorCodes.OptimisticLock),
      _ => (500, "An unexpected error occurred.", ErrorCodes.ServerError)
    };

    if (status >= 500)
      logger.LogError(exception, "Unhandled exception [{ErrorCode}] {Message}", errorCode, message);
    else
      logger.LogWarning(exception, "Handled exception [{ErrorCode}] {Message}", errorCode, message);

    context.Response.StatusCode = status;
    context.Response.ContentType = "application/json";

    object body;

    if (env.IsDevelopment())
    {
      body = new
      {
        success = false,
        status,
        message,
        errorCode,
        exceptionType = exception.GetType().Name,
        stackTrace = exception.ToString()
      };
    }
    else
    {
      body = ApiResponse.Fail(status, message, errorCode);
    }

    await context.Response.WriteAsync(
        JsonSerializer.Serialize(body, JsonOptions),
        cancellationToken);

    return true;
  }
}
