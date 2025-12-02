using CarWare.Application.Common;
using CarWare.Application.DTOs.maintenanceReminder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IMaintenanceReminderService
    {
        Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> GetAllAsync();
        Task<Result<MaintenanceReminderResponseDto>> GetByIdAsync(int id);
        Task<Result<MaintenanceReminderResponseDto>> AddAsync(CreateMaintenanceReminderDto dto);
        Task<Result<MaintenanceReminderResponseDto>> UpdateAsync(UpdateMaintenanceReminderDto dto);
        Task<Result<bool>> DeleteAsync(int id);
        Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> UpcomingMaintenanceAsync(int days = 7);
        Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> GetAllByCarAsync(int vehicleId);
    }
}
