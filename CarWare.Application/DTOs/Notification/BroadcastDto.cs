using CarWare.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Notification
{
    public class BroadcastDto
    {
        [Required] 
        public string Title { get; set; } = null!;

        [Required] 
        public string Body { get; set; } = null!;

        public NotificationChannel Channel { get; set; } = NotificationChannel.Push;
    }
}