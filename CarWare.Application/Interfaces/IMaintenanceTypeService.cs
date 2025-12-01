using CarWare.Application.Common;
using CarWare.Application.DTOs.Maintenance;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IMaintenanceTypeService
    {
        Task<Result<IEnumerable<MaintenanceTypeDto>>> GetAllAsync();
    }
}
