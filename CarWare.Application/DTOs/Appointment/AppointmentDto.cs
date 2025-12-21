using System;

namespace CarWare.Application.DTOs.Appointment
{
     public class AppointmentDto
     {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string VehicleName { get; set; }
        public string ServiceCenterName{ get; set; }
        public string ProviderName { get; set; }
     }
}