using CarWare.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Notification
{
    public class SendNotificationDto
    {
        [Required] 
        public string UserId { get; set; } = null!;
        [Required] 
        public string Title { get; set; } = null!;
        [Required] 
        public string Body { get; set; } = null!;
        public NotificationType Type { get; set; } = NotificationType.General;
        public NotificationChannel Channel { get; set; } = NotificationChannel.Push;
        public string? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }
}
