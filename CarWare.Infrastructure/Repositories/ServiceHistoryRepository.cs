using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
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
            return _context.ServiceRequest.Where(x => x.UserId == userId);
        }
    }
}