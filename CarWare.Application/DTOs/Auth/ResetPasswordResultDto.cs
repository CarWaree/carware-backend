using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class ResetPasswordResultDto
    {
        public string UserID { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string Token { get; set; }
    }
}