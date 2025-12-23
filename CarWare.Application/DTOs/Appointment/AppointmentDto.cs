using CarWare.Domain.Enums;
using System;

namespace CarWare.Application.DTOs.Appointment
{
     public class AppointmentDto
     {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = null!;
        public string VehicleName { get; set; }
        public string ServiceName{ get; set; }
        public string ProviderName { get; set; }
     }
}