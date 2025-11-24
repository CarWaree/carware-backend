using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Auth
{
    public class AuthDto
    {
            public bool IsAuthenticated { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public List<string> Roles { get; set; }
            public string Token { get; set; }
            public DateTime ExpiresOn { get; set; }
    }
}
