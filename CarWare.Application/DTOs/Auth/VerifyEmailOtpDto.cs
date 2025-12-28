using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class VerifyEmailOtpDto
    {
        [Required]
        public string Otp { get; set; } = null!;
    }
}
