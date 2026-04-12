namespace CarWare.Application.DTOs.Auth
{
    public class VerifyEmailResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}