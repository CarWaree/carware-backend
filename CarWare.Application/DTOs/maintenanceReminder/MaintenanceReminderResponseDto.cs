using System;

namespace CarWare.Application.DTOs.maintenanceReminder
{
    public class MaintenanceReminderResponseDto
    {
        public int Id { get; set; }
        public DateTime NotificationDate { get; set; }

        public string TypeName { get; set; }
        public string VehicleName { get; set; }

        public string? Note { get; set; }
    }
}
