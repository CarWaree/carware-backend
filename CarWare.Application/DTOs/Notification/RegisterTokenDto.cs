using CarWare.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Notification
{
    public class RegisterTokenDto
    {
        [Required] 
        public string Token { get; set; } = null!;

        public DevicePlatform Platform { get; set; }
    }
}