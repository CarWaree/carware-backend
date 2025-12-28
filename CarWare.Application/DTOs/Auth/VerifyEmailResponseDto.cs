using System;

namespace CarWare.Application.DTOs.Auth
{
    public class VerifyEmailResponseDto
    {
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
