using System;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.maintenanceReminder
{
    public class CreateMaintenanceReminderDto
    {
        [Required]
        public DateTime NotificationDate { get; set; }
        [Required]
        public DateTime NextDueDate { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        public int VehicleId { get; set; }
    }
}
