using ZLearn.API.Exceptions;

namespace ZLearn.Domain.Exceptions
{
    public class ValidationErrorException : BaseException
    {
        public ValidationErrorException(string message) : base(message, ErrorCodes.VALIDATION_FAILED)
        {
        }
    }
}
