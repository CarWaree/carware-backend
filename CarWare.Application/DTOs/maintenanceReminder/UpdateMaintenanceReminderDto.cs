using System;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.maintenanceReminder
{
    public class UpdateMaintenanceReminderDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime NotificationDate { get; set; }
        public DateTime NextDueDate { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        public int VehicleId { get; set; }
    }
}

