using CarWare.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IServiceRequestRepository : IGenericRepository<ServiceRequest>
    {
        // ── Service Reqeust Screen ──
        IQueryable<ServiceRequest> GetAllQueryable();
        Task<ServiceRequest> GetByIdAsync(int id, int centerId);
        IQueryable<ServiceRequest> GetUserHistory(string userId);
        IQueryable<ServiceRequest> GetByCenterId(int centerId);

        //IQueryable<ServiceRequest> GetCenterHistory(string userId);
    }
}