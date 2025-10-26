using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class ForgetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}