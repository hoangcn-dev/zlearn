namespace Zlearn.V2.Application.Common.DTOs
{
    public class Result<T>
    {
        public Result(bool succeeded, string? message, T? data, string? errorCode)
        {
            Succeeded = succeeded;
            Message = message;
            ErrorCode = errorCode;
            Data = data;
        }

        public T? Data { get; set; }
        public bool Succeeded { get; init; }
        public string? Message { get; init; }
        public string? ErrorCode { get; init; }

        public static Result<T> Success() 
            => new(true, null, default, null);
        public static Result<T> Success(string mess) 
            => new(true, mess, default, null);
        public static Result<T> Success(string? mess, T data) 
            => new(true, mess, data, null);
        public static Result<T> Failure() 
            => new(false, "Something went wrong.", default, ErrorCodes.UNKNOWN_ERROR);
        public static Result<T> Failure(string mess) 
            => new(false, mess, default, ErrorCodes.UNKNOWN_ERROR);
        public static Result<T> Failure(string mess, string errorCode) 
            => new(false, mess, default, errorCode);
    }

    public abstract class NoData { }

    public class ErrorCodes
    {
        public const string UNKNOWN_ERROR = nameof(UNKNOWN_ERROR);
        public const string UNAUTHORIZED = nameof(UNAUTHORIZED);
        public const string FORBIDDEN = nameof(FORBIDDEN);
        public const string INVALID_CREDENTIALS = nameof(INVALID_CREDENTIALS);
        public const string TOKEN_EXPIRED = nameof(TOKEN_EXPIRED);
        public const string ACCOUNT_LOCKED = nameof(ACCOUNT_LOCKED);
        public const string NOT_FOUND = nameof(NOT_FOUND);
        public const string ALREADY_EXISTS = nameof(ALREADY_EXISTS);
        public const string BAD_REQUEST = nameof(BAD_REQUEST);
        public const string RESOURCE_LOCKED = nameof(RESOURCE_LOCKED);
        public const string RESOURCE_DELETED = nameof(RESOURCE_DELETED);
        public const string VALIDATION_FAILED = nameof(VALIDATION_FAILED);
        public const string INVALID_INPUT = nameof(INVALID_INPUT);
        public const string MISSING_REQUIRED_FIELD = nameof(MISSING_REQUIRED_FIELD);
        public const string INVALID_FORMAT = nameof(INVALID_FORMAT);
        public const string INTERNAL_ERROR = nameof(INTERNAL_ERROR);
        public const string REDIRECT = nameof(REDIRECT);
        public const string SERVICE_UNAVAILABLE = nameof(SERVICE_UNAVAILABLE);
        public const string DATABASE_ERROR = nameof(DATABASE_ERROR);
        public const string TIMEOUT = nameof(TIMEOUT);
        public const string OPERATION_REJECTED = nameof(OPERATION_REJECTED);
        public const string BUSINESS_RULE_VIOLATION = nameof(BUSINESS_RULE_VIOLATION);
        public const string INSUFFICIENT_PERMISSIONS = nameof(INSUFFICIENT_PERMISSIONS);
        public const string RATE_LIMIT_EXCEEDED = nameof(RATE_LIMIT_EXCEEDED);
        public const string QUOTA_EXCEEDED = nameof(QUOTA_EXCEEDED);
        public const string FILE_TOO_LARGE = nameof(FILE_TOO_LARGE);
        public const string INVALID_FILE_TYPE = nameof(INVALID_FILE_TYPE);
        public const string UPLOAD_FAILED = nameof(UPLOAD_FAILED);
    }
}

