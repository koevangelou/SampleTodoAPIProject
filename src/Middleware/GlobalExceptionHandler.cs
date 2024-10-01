using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace TodoApi.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {

        private readonly ILogger<GlobalExceptionHandler> _logger;

        public  GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {

            _logger.LogError(exception, exception.Message);
            if (exception.Message.Contains("User not found") || exception.Message.Contains("User not authorized")){
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            httpContext.Response.ContentType = "application/json";

            var details = new ProblemDetails()
            {
                Detail = exception.Message,
                Status = (int)(HttpStatusCode.BadRequest),
                Title = "An error occurred while processing your request",
                Instance = "API"
            };

            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(details), cancellationToken);

            return true;
        }
    }
}
