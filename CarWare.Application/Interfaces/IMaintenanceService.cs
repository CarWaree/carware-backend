using CarWare.Application.Common;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Domain.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IMaintenanceService
    {
        Task<MaintenanceDto> AddAsync(MaintenanceDto dto);
        Task<Pagination<MaintenanceDto>> GetByVehicleIdAsync(int vehicleId, PaginationParameters pagination);
        Task<Pagination<MaintenanceDto>> GetUpcomingAsync(PaginationParameters pagination);
        Task<MaintenanceDto?> UpdateAsync(int id, MaintenanceDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
