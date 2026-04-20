using CarWare.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IServiceRequestRepository : IGenericRepository<ServiceRequest>
    {
        IQueryable<ServiceRequest> GetAllQueryable();
        Task<ServiceRequest> GetByIdAsync(int id);
        IQueryable<ServiceRequest> GetUserHistory(string userId);

        //IQueryable<ServiceRequest> GetCenterHistory(string userId);
    }
}