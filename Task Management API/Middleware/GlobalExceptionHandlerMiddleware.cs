using System.Net;
using System.Text.Json;
using Task_Management_API.DTOs;

namespace Task_Management_API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message),
                InvalidOperationException invOpEx => (HttpStatusCode.Conflict, invOpEx.Message),
                UnauthorizedAccessException unAuthEx => (HttpStatusCode.Unauthorized, unAuthEx.Message),
                KeyNotFoundException notFoundEx => (HttpStatusCode.NotFound, notFoundEx.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.FailResponse(message, (int)statusCode);

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
