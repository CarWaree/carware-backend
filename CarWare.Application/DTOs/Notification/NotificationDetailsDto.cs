using CarWare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Notification
{
    public class NotificationDetailsDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public bool IsRead { get; set; }

        public NotificationType Type { get; set; }

        public NotificationChannel Channel { get; set; }

        public string? ReferenceId { get; set; }

        public string? ReferenceType { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReadAt { get; set; }
    }
}
