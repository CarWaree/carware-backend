using CarWare.Domain.Entities;
using System.Linq;

namespace CarWare.Domain.Interfaces
{
      public interface IMaintenanceRepository :  IGenericRepository<MaintenanceReminder>
      {
        IQueryable<MaintenanceReminder> GetByVehicleIdQueryable(int vehicleId);
        IQueryable<MaintenanceReminder> GetUpcomingQueryable();
      }
}