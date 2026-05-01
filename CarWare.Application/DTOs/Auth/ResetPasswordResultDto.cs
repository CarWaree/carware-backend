using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class ResetPasswordResultDto
    {
        [Required]
        public string ResetPasswordToken { get; set; }
    }
}