using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class VerifyOtpDto
    {
        [Required]
        public string Otp { get; set; }
    }
}