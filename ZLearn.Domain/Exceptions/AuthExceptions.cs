namespace ZLearn.API.Exceptions
{
    // Authentication & Authorization Exceptions
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message = "Authentication is required") 
            : base(message, ErrorCodes.UNAUTHORIZED)
        {
        }
    }

    public class RequireLoginException : BaseException
    {
        public RequireLoginException(string message = "Login is required to access this resource") 
            : base(message, ErrorCodes.UNAUTHORIZED)
        {
        }
    }

    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string message = "You don't have permission to access this resource") 
            : base(message, ErrorCodes.FORBIDDEN)
        {
        }
    }

    public class TokenExpiredException : BaseException
    {
        public TokenExpiredException() 
            : base("Authentication token has expired", ErrorCodes.TOKEN_EXPIRED)
        {
        }
    }

    public class InvalidCredentialsException : BaseException
    {
        public InvalidCredentialsException(string message = "Invalid username or password") 
            : base(message, ErrorCodes.INVALID_CREDENTIALS)
        {
        }
    }

    public class SessionExpiredException : BaseException
    {
        public SessionExpiredException() 
            : base("Your session has expired, please login again", ErrorCodes.SESSION_EXPIRED)
        {
        }
    }
}