using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Auth
{
    public class LoginDto
    {
        
        
            [Required(ErrorMessage = "Email or username is required")]
            [Display(Name = "Email or Username")]
            public string EmailOrUsername { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        
    }
}
