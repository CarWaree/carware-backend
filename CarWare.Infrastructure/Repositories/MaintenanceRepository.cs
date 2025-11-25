using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Repositories
{
    public class MaintenanceRepository : GenericRepository<Maintenance>, IMaintenanceRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MaintenanceRepository(ApplicationDbContext dbContext):base(dbContext) 
        {
            _dbContext = dbContext;
        }

        public IQueryable<Maintenance> GetByVehicleIdQueryable(int vehicleId)
        {
            return _dbContext.maintenances.Where(m => m.VehicleId == vehicleId);

        }

        public IQueryable<Maintenance> GetUpcomingQueryable()
        {
            var today = DateTime.UtcNow;
            return _dbContext.maintenances.Where(m => m.NextDueDate > today).OrderBy(m => m.NextDueDate);
        }
    }
}
