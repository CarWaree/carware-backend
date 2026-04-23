using CarWare.Domain.Entities;
using System;
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
        Task<List<MaintenanceReminder>> GetUpcomingByUserAsync(string userId);
        Task<List<MaintenanceReminder>> GetDueRemindersAsync(DateTime now);
    }
}