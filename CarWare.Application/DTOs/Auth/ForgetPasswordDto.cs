using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class ForgetPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }
}