using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Repositories
{
    public class MaintenanceRepository : GenericRepository<MaintenanceReminder>, IMaintenanceRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MaintenanceRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MaintenanceReminder?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbContext.maintenances
                .Include(r => r.Type)
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Model)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetAllWithDetailsAsync()
        {
            return await _dbContext.maintenances
                .Include(r => r.Type)
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Model)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceReminder>> GetByVehicleWithDetailsAsync(int vehicleId)
        {
            return await _dbContext.maintenances
                .Include(m => m.Type)
                .Include(v => v.Vehicle)
                .Where(v => v.VehicleId == vehicleId)
                .ToListAsync();

        }

        public IQueryable<MaintenanceReminder> GetUpcomingQueryable()
        {
            return _dbContext.maintenances
                    .Include(m => m.Vehicle)
                    .Include(m => m.Type)
                    .OrderBy(m => m.NotificationDate);
        }
    }
}
