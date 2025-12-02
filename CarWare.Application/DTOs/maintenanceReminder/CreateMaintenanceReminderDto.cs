using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
