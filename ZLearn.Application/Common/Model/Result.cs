namespace ZLearn.Application.Common.Model
{
    public class Result<T>
    {
        public Result(bool succeeded, string? message, T? data, IEnumerable<string> errors)
        {
            Succeeded = succeeded;
            Message = message;
            Errors = errors;
            Data = data;
        }

        public T? Data { get; set; }
        public bool Succeeded { get; init; }
        public string? Message { get; init; }
        public IEnumerable<string> Errors { get; init; }

        public static Result<T> Success() 
            => new(true, null, default, Array.Empty<string>());
        public static Result<T> Success(string mess) 
            => new(true, mess, default, Array.Empty<string>());
        public static Result<T> Success(string? mess, T data) 
            => new(true, mess, data, Array.Empty<string>());
        public static Result<T> Failure() 
            => new(false, "Something went wrong.", default, Array.Empty<string>());
        public static Result<T> Failure(string mess) 
            => new(false, mess, default, Array.Empty<string>());
        public static Result<T> Failure(string mess, IEnumerable<string> errors) 
            => new(false, mess, default, errors);
    }

    public abstract class NoData { }
}
