using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IMaintenanceTypeRepository : IGenericRepository<MaintenanceType>
    {
        Task<List<MaintenanceType>> GetByIdsAsync(List<int> ids);
    }
}