using CarWare.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Notification
{
    public class SendMultipleDto
    {
        [Required] 
        public List<string> UserIds { get; set; } = new();

        [Required] 
        public string Title { get; set; } = null!;

        [Required] 
        public string Body { get; set; } = null!;

        public NotificationChannel Channel { get; set; } = NotificationChannel.Push;
    }
}