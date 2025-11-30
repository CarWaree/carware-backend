using CarWare.Application.DTOs.Maintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IMaintenanceTypeService
    {
        Task<IEnumerable<MaintenanceTypeDto>> GetAllAsync();
    }
}
