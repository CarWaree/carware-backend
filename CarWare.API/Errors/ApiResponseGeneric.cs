using CarWare.API.Errors.NonGeneric;

namespace CarWare.API.Errors
{
    public class ApiResponseGeneric<T> : ApiResponse
    {
        public T? Data { get; set; }

        private ApiResponseGeneric(int statusCode, string? message = null, T? data = default)
           : base(statusCode, message)
        {
            Data = data;
        }

        public static ApiResponseGeneric<T> Success(T? data = default, string message = "Success")
            => new ApiResponseGeneric<T>(200, message, data);

        public static ApiResponseGeneric<T> Fail(string message, T? data = default, int statusCode = 400)
            => new ApiResponseGeneric<T>(statusCode, message, data);
    }
}