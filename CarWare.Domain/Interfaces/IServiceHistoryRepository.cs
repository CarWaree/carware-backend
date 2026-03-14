using CarWare.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IServiceHistoryRepository : IGenericRepository<ServiceRequest>
    {
        IQueryable<ServiceRequest> GetUserHistory(string userId);
    }
}