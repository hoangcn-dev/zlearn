using System;

namespace Zlearn.V2.Application.Common.Exceptions
{
    public abstract class BaseException : Exception
    {
        public string ErrorCode { get; }

        protected BaseException(string message, string errorCode, Exception? innerException = null) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(message, "NOT_FOUND") { }
        public NotFoundException(string resourceType, string resourceId)
            : base($"{resourceType} with id '{resourceId}' was not found", "NOT_FOUND") { }
    }

    public class BadRequestException : BaseException
    {
        public BadRequestException(string message) : base(message, "BAD_REQUEST") { }
        public BadRequestException(string message, string errorCode) : base(message, errorCode) { }
    }

    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string message = "You don't have permission to access this resource") : base(message, "FORBIDDEN") { }
    }

    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message = "Unauthorized access.") : base(message, "UNAUTHORIZED") { }
    }

    public class ValidationErrorException : BaseException
    {
        public ValidationErrorException(string message) : base(message, "VALIDATION_FAILED") { }
    }

    public class DuplicateEntryException : BaseException
    {
        public DuplicateEntryException(string entityType, string identifier)
            : base($"A {entityType} with the same {identifier} already exists", "ALREADY_EXISTS") { }
    }

    public class ResourceConflictException : BaseException
    {
        public ResourceConflictException(string message) : base(message, "RESOURCE_LOCKED") { }
    }

    public class InternalErrorException : BaseException
    {
        public InternalErrorException(string message = "An unexpected error occurred", Exception? innerException = null)
            : base(message, "INTERNAL_ERROR") { }
    }

    public class DatabaseErrorException : BaseException
    {
        public DatabaseErrorException(string message, Exception? innerException = null)
            : base(message, "DATABASE_ERROR") { }
    }

    public class RedirectException : BaseException
    {
        public string Url { get; set; }
        public RedirectException(string url) : base("Redirect to " + url, "REDIRECT")
        {
            Url = url;
        }
    }

    public class TimeoutException : BaseException
    {
        public TimeoutException(string operation) : base($"Operation '{operation}' timed out", "TIMEOUT") { }
    }

    public class TokenExpiredException : BaseException
    {
        public TokenExpiredException() 
            : base("Authentication token has expired", "TOKEN_EXPIRED")
        {
        }
    }

    public class InvalidCredentialsException : BaseException
    {
        public InvalidCredentialsException(string message = "Invalid username or password") 
            : base(message, "INVALID_CREDENTIALS")
        {
        }
    }

    public class ServiceUnavailableException : BaseException
    {
        public ServiceUnavailableException(string serviceName)
            : base($"The {serviceName} service is currently unavailable", "SERVICE_UNAVAILABLE")
        {
        }
    }
}

