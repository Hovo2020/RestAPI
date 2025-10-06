using RestAPI.Constants;

namespace RestAPI.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public string Details { get; }

        public ApiException(string message, int statusCode, string errorCode, string details = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Details = details;
        }
    }

    public class NotFoundException : ApiException
    {
        public NotFoundException(string resourceName, string resourceId)
            : base($"{resourceName} with id {resourceId} was not found", 404, ErrorCodes.NotFound)
        {
        }
    }

    public class ArgumentException : ApiException
    {
        public ArgumentException(string message, string parameterName)
            : base($"{message}. Parameter name: {parameterName}", 400, ErrorCodes.InvalidArgument)
        {
        }
    }

    public class ValidationException : ApiException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(Dictionary<string, string[]> errors)
            : base(ErrorTitles.ValidationFailed, 400, ErrorCodes.ValidationError)
        {
            Errors = errors;
        }
    }

    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message = ErrorTitles.Unauthorized)
            : base(message, 401, ErrorCodes.Unauthorized)
        {
        }
    }

    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message = ErrorTitles.Forbidden)
            : base(message, 403, ErrorCodes.Forbidden)
        {
        }
    }

    public class ConflictException : ApiException
    {
        public ConflictException(string message)
            : base(message, 409, ErrorCodes.Conflict)
        {
        }
    }

    public class BusinessRuleException : ApiException
    {
        public BusinessRuleException(string message)
            : base(message, 422, ErrorCodes.BusinessRuleViolation)
        {
        }
    }

    // Additional exception for database errors
    public class DatabaseException : ApiException
    {
        public DatabaseException(string message, Exception innerException = null)
            : base(message, 500, ErrorCodes.DatabaseError, innerException?.Message)
        {
        }
    }

    // Additional exception for external service errors
    public class ExternalServiceException : ApiException
    {
        public ExternalServiceException(string serviceName, string message)
            : base($"Error calling {serviceName}: {message}", 502, "EXTERNAL_SERVICE_ERROR")
        {
        }
    }
}