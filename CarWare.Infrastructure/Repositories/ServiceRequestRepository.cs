using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Repositories
{
    public class ServiceRequestRepository : GenericRepository<ServiceRequest>, IServiceRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<ServiceRequest> GetAllQueryable()
        {
            return _context.ServiceRequest
                .Include(x => x.User)
                .Include(x => x.Vehicle)
                .Include(x => x.ServiceCenter)
                .Include(x => x.Appointment)
                .Include(x => x.ServiceRequestServices)
                    .ThenInclude(s => s.MaintenanceType);
        }

        public async Task<ServiceRequest> GetByIdAsync(int id)
        {
            return await _context.ServiceRequest
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public IQueryable<ServiceRequest> GetUserHistory(string userId)
        {
            return _context.ServiceRequest
                .Where(x => x.UserId == userId &&
                       (x.ServiceStatus == ServiceRequestStatus.Completed ||
                        x.ServiceStatus == ServiceRequestStatus.Rejected))
                .Include(x => x.Vehicle)
                .Include(x => x.ServiceCenter)
                .Include(x => x.ServiceRequestServices)
                    .ThenInclude(s => s.MaintenanceType);
        }
    }
}