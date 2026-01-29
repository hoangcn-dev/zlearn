namespace ZLearn.Application.Common.DTOs
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
}
