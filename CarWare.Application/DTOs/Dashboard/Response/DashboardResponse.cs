using CarWare.Application.DTOs.Dashboard.Calendar;
using System.Collections.Generic;

namespace CarWare.Application.DTOs.Dashboard.Response
{
    public class DashboardResponse
    {
        public StatCardDto Stats { get; set; }
        public List<CalendarDayDto> Calendar { get; set; }
    }
}