using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class LoginDto
    {   
            [Required(ErrorMessage = "Email or username is required")]
            public string EmailOrUsername { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        
    }
}