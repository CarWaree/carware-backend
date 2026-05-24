using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using CarWare.Infrastructure.Context;
using System.Threading.Tasks;
using System.Linq;

namespace CarWare.Infrastructure.Repositories
{
    public class ServiceCenterRepository
        : GenericRepository<ServiceCenter>, IServiceCenterRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceCenterRepository(ApplicationDbContext context) : base(context)
            => _context = context;

        //Find Center by Admin
        public Task<ServiceCenter?> GetByAdminUserIdAsync(string userId) =>
            _context.ServiceCenters
                    .Include(c => c.Admins)
                    .Include(c => c.ProviderServices)
                    .FirstOrDefaultAsync(c => c.Admins.Any(a => a.Id == userId));

        //Return Center with All Details
        public Task<ServiceCenter?> GetWithDetailsAsync(int id) =>
            _context.ServiceCenters
                    .Include(c => c.ProviderServices).ThenInclude(ps => ps.Service)
                    .Include(c => c.Slots)
                    .FirstOrDefaultAsync(c => c.Id == id);
    }
}