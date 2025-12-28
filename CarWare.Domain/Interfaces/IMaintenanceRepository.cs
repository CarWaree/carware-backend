using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IMaintenanceRepository : IGenericRepository<MaintenanceReminder>
    {
        Task<IEnumerable<MaintenanceReminder>> GetAllWithDetailsAsync();
        Task<MaintenanceReminder?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<MaintenanceReminder>> GetByVehicleWithDetailsAsync(int vehicleId);
        IQueryable<MaintenanceReminder> GetUpcomingQueryable(int days = 7);
    }
}