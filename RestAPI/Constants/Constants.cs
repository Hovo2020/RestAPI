namespace RestAPI.Constants
{
    public static class ValidationMessages
    {
        // Field names
        public const string FieldName = "Name";
        public const string FieldEmail = "Email";
        public const string FieldAge = "Age";
        public const string FieldPassword = "Password";
        public const string FieldConfirmPassword = "Confirm Password";
        public const string FieldStreet = "Street";
        public const string FieldCity = "City";
        public const string FieldCountry = "Country";
        public const string FieldPostalCode = "Postal Code";

        // Validation messages
        public const string RequiredField = "{0} is required";
        public const string StringLength = "{0} must be between {1} and {2} characters";
        public const string EmailInvalid = "Invalid email format";
        public const string AgeRange = "Age must be between {0} and {1}";
        public const string PasswordMinLength = "Password must be at least 8 characters long";
        public const string PasswordUppercase = "Password must contain at least one uppercase letter";
        public const string PasswordLowercase = "Password must contain at least one lowercase letter";
        public const string PasswordDigit = "Password must contain at least one digit";
        public const string PasswordSpecialChar = "Password must contain at least one special character";
        public const string PasswordsDoNotMatch = "Passwords do not match";
        public const string NameAlphaOnly = "Name can only contain letters and spaces";
        public const string EmailAlreadyExists = "Email address is already registered";
        public const string InvalidCreationDate = "Creation date cannot be in the future";
        public const string StreetTooLong = "Street address is too long";
        public const string CityTooLong = "City name is too long";
        public const string CountryTooLong = "Country name is too long";
        public const string InvalidPostalCode = "Postal code format is invalid";
        public const string NameTooLong = "Name is too long";
    }

    public static class ErrorCodes
    {
        public const string ValidationError = "VALIDATION_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string InvalidArgument = "INVALID_ARGUMENT";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string Conflict = "CONFLICT";
        public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
        public const string InternalError = "INTERNAL_ERROR";
        public const string DatabaseError = "DATABASE_ERROR";
        public const string CreateUserError = "CREATE_USER_ERROR";
    }

    public static class ErrorTitles
    {
        public const string ValidationFailed = "Validation failed";
        public const string NotFound = "Resource not found";
        public const string Unauthorized = "Unauthorized access";
        public const string Forbidden = "Access forbidden";
        public const string Conflict = "Resource conflict";
        public const string BusinessRuleViolation = "Business rule violation";
        public const string InternalServerError = "Internal server error";
    }

    public static class SuccessMessages
    {
        public const string UserCreated = "User created successfully";
        public const string UserUpdated = "User updated successfully";
        public const string UserDeleted = "User deleted successfully";
        public const string CustomerCreated = "Customer created successfully";
    }

    public static class LogMessages
    {
        public const string UserCreated = "User created with ID {UserId}";
        public const string UserUpdated = "User with ID {UserId} was updated";
        public const string UserDeleted = "User with ID {UserId} was deleted";
        public const string UserNotFound = "User with ID {UserId} was not found";
        public const string ApiExceptionOccurred = "API Exception occurred: {Message}";
        public const string ValidationExceptionOccurred = "Validation exception occurred";
        public const string FluentValidationExceptionOccurred = "FluentValidation exception occurred";
        public const string UnhandledExceptionOccurred = "Unhandled exception occurred. ErrorId: {ErrorId}";
    }

    public static class ResourceNames
    {
        public const string User = "User";
        public const string Customer = "Customer";
    }
}