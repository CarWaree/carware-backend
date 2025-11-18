using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Auth
{
    public class ResetPasswordResultDto
    {
        public string Token { get; set; }
    }
}