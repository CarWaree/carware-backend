using System;

namespace CarWare.Application.DTOs.Appointment
{
    public class CreateAppointmentDto
    {
        public DateTime Date { get; set; }
        public string TimeSlot { get; set; }

        // FK
        public int VehicleId { get; set; }
        public int serviceId { get; set; }
        public int ServiceCenterId { get; set; }
    }
}
