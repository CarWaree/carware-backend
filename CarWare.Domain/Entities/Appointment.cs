using CarWare.Domain.Enums;
using System;

namespace CarWare.Domain.Entities
{
    public class Appointment : BaseEntity
    {
        public DateTime Date { get; set; }
        public string TimeSlot { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        //FK
        public string UserId { get; set; }
        public int VehicleId { get; set; }
        public int ServiceCenterId { get; set; }

        //Navigation 
        public ApplicationUser user { get; set; }
        public Vehicle Vehicle { get; set; }
        public ServiceCenter ServiceCenter { get; set; }

        //Optional 
        public int? MaintenanceReminderId { get; set; }
        public MaintenanceReminder? MaintenanceReminder { get; set; }
    }
}