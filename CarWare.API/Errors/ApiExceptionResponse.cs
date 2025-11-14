namespace CarWare.API.Errors
{
    public class ApiExceptionResponse : ApiResponse
    {
        public string Description { get; set; }
        public ApiExceptionResponse(int statusCode, string? message = null, string? Description = null)
            :base(statusCode, message)
        {
            this.Description = Description;
        }
    }
}