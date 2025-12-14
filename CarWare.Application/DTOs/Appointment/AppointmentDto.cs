using CarWare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Appointment
{
     public class AppointmentDto
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public string TimeSlot { get; set; }
        public AppointmentStatus Status { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }

        public int VehicleId { get; set; }
        public string VehicleName { get; set; }

        public int ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; }

        public int? MaintenanceReminderId { get; set; }
    }
}
