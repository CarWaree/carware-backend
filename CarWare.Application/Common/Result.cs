namespace CarWare.Application.Common
{
    public class Result<T>
    {
        public bool Success { get; }
        public string? Error { get; }
        public string ErrorCode { get; }
        public T? Data { get; }

        private Result(bool success, T? data, string? error, string? errorCode)
        {
            Success = success;
            Error = error;
            ErrorCode = errorCode;
            Data = data;
        }

        public static Result<T> Ok(T data)
        => new(true, data, null, null);

        public static Result<T> Fail(string error, string errorCode = "BadRequest")
            => new(false, default, error, errorCode);
    }
}
