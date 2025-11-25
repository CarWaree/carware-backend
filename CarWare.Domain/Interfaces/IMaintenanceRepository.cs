using CarWare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
      public interface IMaintenanceRepository:IGenericRepository<Maintenance>
      {
        IQueryable<Maintenance> GetByVehicleIdQueryable(int vehicleId);
        IQueryable<Maintenance> GetUpcomingQueryable();

    }
}
