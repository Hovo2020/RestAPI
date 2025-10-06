using System.Diagnostics;

namespace RestAPI.Models
{
    public class ErrorResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new();
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;

        public ErrorResponse()
        {
            Timestamp = DateTime.UtcNow;
        }

        public static ErrorResponse Create(HttpContext context, string title, int status, string detail = null, string errorCode = null)
        {
            return new ErrorResponse
            {
                Type = GetErrorType(status),
                Title = title,
                Status = status,
                Detail = detail ?? string.Empty,
                ErrorCode = errorCode ?? string.Empty,
                TraceId = context.TraceIdentifier,
                Instance = context.Request.Path,
                Path = context.Request.Path,
                Method = context.Request.Method
            };
        }

        private static string GetErrorType(int statusCode)
        {
            return statusCode switch
            {
                400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
                403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
                500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            };
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> Ok(T data, string message = "")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Message = message
            };
        }
    }
}