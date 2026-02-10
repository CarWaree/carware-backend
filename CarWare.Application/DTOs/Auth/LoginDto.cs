using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email or username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Email or username must be between 3 and 100 characters")]
        public string EmailOrUsername { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}