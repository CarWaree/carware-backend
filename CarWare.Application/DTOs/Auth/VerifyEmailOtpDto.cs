using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Auth
{
    public class VerifyEmailOtpDto
    {
      

        [Required]
        public string Otp { get; set; } = null!;
    }
}
