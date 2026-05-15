using System.Collections.Generic;

namespace CarWare.Application.DTOs.Dashboard.Responses;

public class DashboardResponse
{
    public decimal TodayIncome { get; set; }
    public List<CalendarDayDto> WeeklySchedule { get; set; } = [];
}