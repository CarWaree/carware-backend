using CarWare.Domain.Entities;
using System.Linq;

namespace CarWare.Domain.Interfaces
{
      public interface IMaintenanceRepository :  IGenericRepository<Maintenance>
      {
        IQueryable<Maintenance> GetByVehicleIdQueryable(int vehicleId);
        IQueryable<Maintenance> GetUpcomingQueryable();
      }
}