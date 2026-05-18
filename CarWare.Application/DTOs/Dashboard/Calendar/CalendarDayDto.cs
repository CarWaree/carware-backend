using System;
using System.Collections.Generic;

namespace CarWare.Application.DTOs.Dashboard.Calendar
{
    public class CalendarDayDto
    {
        public string DayName { get; set; }

        public DateTime Date { get; set; }

        public List<CalendarAppointmentDto> Slots { get; set; }
    }
}