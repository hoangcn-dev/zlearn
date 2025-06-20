namespace ZLearn.API.Exceptions
{
    public abstract class BaseException : Exception
    {
        protected BaseException(string mess, string errorCode) : base(mess)
        {
            ErrorCode = errorCode;
        }

        protected BaseException(string mess, string errorCode, Exception innerException) : base(mess, innerException)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }

        public class ErrorCodes
        {
            // Authentication & Authorization
            public const string UNAUTHORIZED = nameof(UNAUTHORIZED);
            public const string FORBIDDEN = nameof(FORBIDDEN);
            public const string INVALID_CREDENTIALS = nameof(INVALID_CREDENTIALS);
            public const string TOKEN_EXPIRED = nameof(TOKEN_EXPIRED);
            public const string SESSION_EXPIRED = nameof(SESSION_EXPIRED);

            // Resource errors
            public const string NOT_FOUND = nameof(NOT_FOUND);
            public const string ALREADY_EXISTS = nameof(ALREADY_EXISTS);
            public const string RESOURCE_LOCKED = nameof(RESOURCE_LOCKED);
            public const string RESOURCE_DELETED = nameof(RESOURCE_DELETED);

            // Validation errors
            public const string VALIDATION_FAILED = nameof(VALIDATION_FAILED);
            public const string INVALID_INPUT = nameof(INVALID_INPUT);
            public const string MISSING_REQUIRED_FIELD = nameof(MISSING_REQUIRED_FIELD);
            public const string INVALID_FORMAT = nameof(INVALID_FORMAT);
            
            // Server errors
            public const string INTERNAL_ERROR = nameof(INTERNAL_ERROR);
            public const string SERVICE_UNAVAILABLE = nameof(SERVICE_UNAVAILABLE);
            public const string DATABASE_ERROR = nameof(DATABASE_ERROR);
            public const string TIMEOUT = nameof(TIMEOUT);
            
            // Business logic errors
            public const string OPERATION_REJECTED = nameof(OPERATION_REJECTED);
            public const string BUSINESS_RULE_VIOLATION = nameof(BUSINESS_RULE_VIOLATION);
            public const string INSUFFICIENT_PERMISSIONS = nameof(INSUFFICIENT_PERMISSIONS);
            
            // Rate limiting/quota errors
            public const string RATE_LIMIT_EXCEEDED = nameof(RATE_LIMIT_EXCEEDED);
            public const string QUOTA_EXCEEDED = nameof(QUOTA_EXCEEDED);
            
            // File/Upload errors
            public const string FILE_TOO_LARGE = nameof(FILE_TOO_LARGE);
            public const string INVALID_FILE_TYPE = nameof(INVALID_FILE_TYPE);
            public const string UPLOAD_FAILED = nameof(UPLOAD_FAILED);
        }
    }
}
