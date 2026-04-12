using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class VerifyOtpDto
    {
        public string Email { get; set; }
        [Required]
        public string Otp { get; set; }
    }
}