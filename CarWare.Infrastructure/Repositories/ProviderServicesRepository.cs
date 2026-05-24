using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Repositories
{
    public class ProviderServicesRepository : IProviderServicesRepository
    {
        private readonly ApplicationDbContext _context;
        public ProviderServicesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(List<ProviderServices> providers)
            => await _context.ProviderServices.AddRangeAsync(providers);

        public Task<List<ProviderServices>> GetByCenterIdAsync(int centerId) =>
            _context.ProviderServices
                    .Include(ps => ps.Service)
                    .Where(ps => ps.ServiceCenterId == centerId)
                    .ToListAsync();

        public async Task DeleteByCenterIdAsync(int centerId)
        {
            var existing = await _context.ProviderServices
                .Where(ps => ps.ServiceCenterId == centerId)
                .ToListAsync();

            _context.ProviderServices.RemoveRange(existing);
        }
    }
}