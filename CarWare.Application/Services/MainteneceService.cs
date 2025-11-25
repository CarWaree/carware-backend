using CarWare.Application.Common;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.Interfaces;
using CarWare.Domain.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class MainteneceService : IMaintenanceService
    {
        public Task<MaintenanceDto> AddAsync(MaintenanceDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Pagination<MaintenanceDto>> GetByVehicleIdAsync(int vehicleId, PaginationParameters pagination)
        {
            throw new NotImplementedException();
        }

        public Task<Pagination<MaintenanceDto>> GetUpcomingAsync(PaginationParameters pagination)
        {
            throw new NotImplementedException();
        }

        public Task<MaintenanceDto> UpdateAsync(int id, MaintenanceDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
