using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CarWare.Infrastructure.Repositories
{
    public class ServiceHistoryRepository : GenericRepository<ServiceRequest>, IServiceHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceHistoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<ServiceRequest> GetUserHistory(string userId)
        {
            return _context.ServiceRequest
                .Where(x => x.UserId == userId)
                .Include(x => x.Vehicle)
                .Include(x => x.ServiceCenter)
                .Include(x => x.ServiceRequestServices)
                    .ThenInclude(s => s.MaintenanceType);
        }
    }
}