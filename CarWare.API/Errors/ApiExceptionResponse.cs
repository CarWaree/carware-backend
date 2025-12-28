using CarWare.API.Errors.NonGeneric;

namespace CarWare.API.Errors
{
    public class ApiExceptionResponse : ApiResponse
    {
        public string? Description { get; set; }
        public ApiExceptionResponse(int statusCode, string? message = null, string? description = null)
            : base(statusCode, message)
        {
            description = Description;
        }
    }
}