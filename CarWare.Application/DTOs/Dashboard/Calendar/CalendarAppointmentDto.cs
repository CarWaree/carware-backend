using System;

namespace CarWare.Application.DTOs.Dashboard.Calendar
{
    public class CalendarAppointmentDto
    {
        public int AppointmentId { get; set; }
        public string ServiceType { get; set; }
        public string CarName { get; set; }  
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}