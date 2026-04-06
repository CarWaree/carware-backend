using CarWare.Domain.Enums;
using System;

namespace CarWare.Domain.Entities
{
    public class Notification : BaseEntity
    {
        //FK
        public string UserId { get; set; }
        //Navigation Property
        public ApplicationUser User { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }

        public NotificationType Type { get; set; }

        public NotificationChannel Channel { get; set; }

        public bool IsRead { get; set; } = false;
        public bool IsSent { get; set; } = false;

        public string? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }

        public string? DataJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
