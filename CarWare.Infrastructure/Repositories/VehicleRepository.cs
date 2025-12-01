using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public VehicleRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Vehicle>> GetAllCarsWithDetailsAsync()
        {
            return await _dbContext.vehicles
                .Include(c => c.Brand)
                .Include(c => c.Model)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetCarByIdWithDetailsAsync(int id)
        {
            return await _dbContext.vehicles
                .Include(c => c.Brand)
                .Include(c => c.Model)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Model>> GetModelsByBrandAsync(int brandId)
        {
            var models = await _dbContext.models
                .Include(m => m.Brand)
                .Where(m => m.BrandId == brandId)
                .ToListAsync();

            return models;
        }
    }
}
