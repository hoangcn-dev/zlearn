namespace ZLearn.API.Exceptions
{
    // Server Exceptions
    public class InternalErrorException : BaseException
    {
        public InternalErrorException(string message = "An unexpected error occurred", Exception innerException = null)
            : base(message, ErrorCodes.INTERNAL_ERROR, innerException)
        {
        }
    }

    public class DatabaseErrorException : BaseException
    {
        public DatabaseErrorException(string message, Exception innerException = null)
            : base(message, ErrorCodes.DATABASE_ERROR, innerException)
        {
        }
    }

    public class ServiceUnavailableException : BaseException
    {
        public ServiceUnavailableException(string serviceName)
            : base($"The {serviceName} service is currently unavailable", ErrorCodes.SERVICE_UNAVAILABLE)
        {
        }
    }

    public class TimeoutException : BaseException
    {
        public TimeoutException(string operation)
            : base($"Operation '{operation}' timed out", ErrorCodes.TIMEOUT)
        {
        }
    }
}