using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Auth
{
    public class ResetPasswordResultDto
    {
        public string UserID { get; set; }

        public string Email { get; set; }

        public string Token { get; set; }
    }
}
