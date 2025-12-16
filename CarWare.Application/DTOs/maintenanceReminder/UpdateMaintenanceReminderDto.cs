using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

