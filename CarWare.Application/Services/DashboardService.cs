using CarWare.Application.Common;
using CarWare.Application.DTOs.Dashboard;
using CarWare.Application.DTOs.Dashboard.Calendar;
using CarWare.Application.DTOs.Dashboard.Response;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services.ServiceRequests
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DashboardService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        private int CenterId => _currentUserService.ServiceCenterId
            ?? throw new Exception("ServiceCenterId not found");

        public async Task<Result<DashboardResponse>> GetDashboardAsync(DashboardQueryParams queryParams)
        {
            // ── Stat cards ───────
            var requestCount = await _unitOfWork.ServiceRequestRepository
                .CountTodayByCenterIdAsync(CenterId);

            var todayIncome = await _unitOfWork.ServiceRequestRepository
                .SumTodayIncomeByCenterIdAsync(CenterId);

            // ── Calendar window (default = current week starting Sunday) ───
            var weekStart = queryParams.WeekStart?.Date
                ?? GetCurrentWeekStart();
            var weekEnd = weekStart.AddDays(7);

            var appointments = await _unitOfWork.ServiceRequestRepository
                .GetWeeklyAppointments(CenterId, weekStart, weekEnd)
                .ToListAsync();

            // ── Build calendar days (Sun → Sat) ────────
            var calendarDays = Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var day = weekStart.AddDays(offset);

                    var slots = appointments
                        .Where(x => x.Appointment!.Date.Date == day.Date)
                        .OrderBy(x => x.Appointment!.Date.TimeOfDay)
                        .Select(x => new CalendarAppointmentDto
                        {
                            AppointmentId = x.Appointment!.Id,
                            ServiceType = x.ServiceRequestServices
                                              .Select(s => s.MaintenanceType.Name)
                                              .FirstOrDefault() ?? "—",
                            CarName = $"{x.Vehicle.Brand.Name} {x.Vehicle.Model.Name}",
                            StartTime = x.Appointment.Date.TimeOfDay,
                            EndTime = x.Appointment.Date.AddHours(1).TimeOfDay
                        })
                        .ToList();

                    return new CalendarDayDto
                    {
                        Date = day,
                        DayName = day.ToString("dddd"),   // "Sunday", "Monday" …
                        Slots = slots
                    };
                })
                .ToList();

            var response = new DashboardResponse
            {
                Stats = new StatCardDto
                {
                    ServiceRequests = requestCount,
                    TotalIncomeToday = todayIncome
                },
                Calendar = calendarDays
            };

            return Result<DashboardResponse>.Ok(response);
        }

        // ── Helpers ───
        private static DateTime GetCurrentWeekStart()
        {
            var today = DateTime.UtcNow.Date;
            var diff = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Sunday) % 7;
            return today.AddDays(-diff);
        }
    }
}