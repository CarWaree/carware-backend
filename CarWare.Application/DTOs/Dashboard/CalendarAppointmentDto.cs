using System;

namespace CarWare.Application.DTOs.Dashboard
{
    public class CalendarAppointmentDto
    {
        public int ServiceRequestId { get; set; }

        public string ServiceType { get; set; } = string.Empty;

        public string CarName { get; set; } = string.Empty;

        public DateTime AppointmentTime { get; set; }

        public DateTime Date { get; set; }
    }
}