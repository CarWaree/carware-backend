namespace CarWare.API.Errors.NonGeneric
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }

        protected ApiResponse(int statusCode, string? message)
        {
            StatusCode = statusCode;
            Message = message;
        }

        public static ApiResponse Success(string message = "Success", int statusCode = 200)
            => new ApiResponse(statusCode, message);

        public static ApiResponse Fail(string message, int statusCode = 400)
            => new ApiResponse(statusCode, message);
    }
}