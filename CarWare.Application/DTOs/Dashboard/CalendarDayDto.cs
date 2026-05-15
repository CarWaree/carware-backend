using System;
using System.Collections.Generic;

namespace CarWare.Application.DTOs.Dashboard
{
    public class CalendarDayDto
    {
        public string DayName { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public List<CalendarAppointmentDto> Appointments { get; set; } = [];
    }
}