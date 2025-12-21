using CarWare.Application.Common;
using CarWare.Application.DTOs.Appointment;
using CarWare.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<Result<AppointmentDto>> CancelAsync(int id, string userId);
        Task<Result<AppointmentDto>> UpdateStatusAsync(int id, AppointmentStatus status);
        Task<Result<List<AppointmentDto>>> GetUserAppointmentsAsync(string userId);
        Task<Result<AppointmentDto>> AddAppointmentAsync(CreateAppointmentDto dto, string userId);
    }
}