using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Appointment
{
    public class CreateAppointmentDto
    {
        public DateTime Date { get; set; }
        public string TimeSlot { get; set; }

        // FK
        public string UserId { get; set; }
        public int VehicleId { get; set; }
        public int ServiceCenterId { get; set; }

        // Optional
        public int? MaintenanceReminderId { get; set; }
    
}
}
