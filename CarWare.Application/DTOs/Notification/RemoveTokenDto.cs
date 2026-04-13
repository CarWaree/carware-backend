using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Notification
{
    public class RemoveTokenDto
    {
        [Required]
        public string Token { get; set; } = null!;
    }
}