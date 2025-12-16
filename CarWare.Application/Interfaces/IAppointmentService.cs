using CarWare.Application.Common;
using CarWare.Application.DTOs.Appointment;
using CarWare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<Result<AppointmentDto>> CancelAsync(int id, string userId);
        Task<Result<AppointmentDto>> UpdateStatusAsync(int id, AppointmentStatus status);
        Task<Result<List<AppointmentDto>>> GetByUserIdAsync(string userId);
    }
}
