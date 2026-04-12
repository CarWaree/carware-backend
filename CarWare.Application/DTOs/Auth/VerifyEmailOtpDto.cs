using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class VerifyEmailOtpDto
    {
        public string Email { get; set; }

        [Required]
        public string Otp { get; set; } = null!;
    }
}
