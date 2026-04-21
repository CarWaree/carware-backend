using System;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.maintenanceReminder
{
    public class CreateMaintenanceReminderDto
    {
        [Required]
        public DateTime NotificationDate { get; set; }

        public int? RepeatInterval { get; set; }
        public RepeatUnit? RepeatUnit { get; set; }
        public int? RepeatCount { get; set; }

        public string? Note { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        public int VehicleId { get; set; }
    }
}
