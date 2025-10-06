using FluentValidation;
using RestAPI.Exceptions;
using RestAPI.Models;
using System.Diagnostics;

namespace RestAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = exception switch
            {
                ApiException apiEx => HandleApiException(context, apiEx),
                FluentValidation.ValidationException fluentEx => HandleFluentValidationException(context, fluentEx),
                _ => HandleUnhandledException(context, exception)
            };

            response.StatusCode = errorResponse.Status;
            await response.WriteAsJsonAsync(errorResponse);
        }

        private ErrorResponse HandleApiException(HttpContext context, ApiException exception)
        {
            _logger.LogWarning(exception, "API Exception occurred: {Message}", exception.Message);

            return ErrorResponse.Create(
                context,
                exception.Message,
                exception.StatusCode,
                exception.Details,
                exception.ErrorCode);
        }

        private ErrorResponse HandleValidationException(HttpContext context, Exceptions.ValidationException exception)
        {
            _logger.LogWarning(exception, "Validation exception occurred");

            var errorResponse = ErrorResponse.Create(
                context,
                exception.Message,
                exception.StatusCode,
                errorCode: exception.ErrorCode);

            errorResponse.Errors = exception.Errors;
            return errorResponse;
        }

        private ErrorResponse HandleFluentValidationException(HttpContext context, FluentValidation.ValidationException exception)
        {
            _logger.LogWarning(exception, "FluentValidation exception occurred");

            var errors = exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation failed",
                Status = 400,
                Detail = "One or more validation errors occurred",
                ErrorCode = "VALIDATION_ERROR",
                TraceId = context.TraceIdentifier,
                Instance = context.Request.Path,
                Errors = errors,
                Path = context.Request.Path,
                Method = context.Request.Method
            };
        }

        private ErrorResponse HandleUnhandledException(HttpContext context, Exception exception)
        {
            var errorId = Guid.NewGuid().ToString();
            _logger.LogError(exception, "Unhandled exception occurred. ErrorId: {ErrorId}", errorId);

            var detail = _environment.IsDevelopment()
                ? exception.ToString()
                : $"An unexpected error occurred. Reference: {errorId}";

            return ErrorResponse.Create(
                context,
                "An internal server error occurred",
                500,
                detail,
                "INTERNAL_ERROR");
        }
    }
}