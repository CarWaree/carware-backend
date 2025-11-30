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

        //public async Task<List<string>> GetAllBrandsAsync()
        //{
        //    var brands = await _dbContext.vehicles
        //        .Select(v => v.Brand)
        //        .Distinct()
        //        .ToListAsync();

        //    return brands;
        //}

        //public async Task<List<Vehicle>> GetModelsByBrandAsync(string brandName)
        //{
        //    return await _dbContext.vehicles
        //        .Where(v => v.Brand == brandName)
        //        .ToListAsync();
        //}
    }
}
