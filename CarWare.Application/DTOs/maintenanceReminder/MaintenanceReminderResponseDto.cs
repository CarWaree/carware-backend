using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.maintenanceReminder
{
    public class MaintenanceReminderResponseDto
    {
        public int Id { get; set; }
        public DateTime NotificationDate { get; set; }
        public DateTime NextDueDate { get; set; }
        public string TypeName { get; set; }
        public string VehicleName { get; set; }
    
}
}
