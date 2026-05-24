using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Repositories
{
    public class MaintenanceTypeRepository
        : GenericRepository<MaintenanceType>, IMaintenanceTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceTypeRepository(ApplicationDbContext context) : base(context)
            => _context = context;

        //Get All Services within List Of IDs 
        public Task<List<MaintenanceType>> GetByIdsAsync(List<int> ids) =>
            _context.MaintenanceTypes
                    .Where(m => ids.Contains(m.Id))
                    .ToListAsync();
    }
}