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

        public static ApiResponse Success(string message = "Success")
            => new ApiResponse(200, message);

        public static ApiResponse Fail(string message, int statusCode = 400)
            => new ApiResponse(statusCode, message);
    }
}