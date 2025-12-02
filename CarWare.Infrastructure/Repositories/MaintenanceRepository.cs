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
    public class MaintenanceRepository : GenericRepository<MaintenanceReminder>, IMaintenanceRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MaintenanceRepository(ApplicationDbContext dbContext):base(dbContext) 
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

        public IQueryable<MaintenanceReminder> GetByVehicleIdQueryable(int vehicleId)
        {
            return _dbContext.maintenances.Where(m => m.VehicleId == vehicleId);

        }

        public IQueryable<MaintenanceReminder> GetUpcomingQueryable()
        {
            var today = DateTime.UtcNow;
            return _dbContext.maintenances.Where(m => m.NextDueDate > today).OrderBy(m => m.NextDueDate);
        }
    }
}
