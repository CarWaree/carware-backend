using CarWare.Domain.Entities;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IServiceCenterRepository : IGenericRepository<ServiceCenter>
    {
        Task<ServiceCenter?> GetByAdminUserIdAsync(string userId);
        Task<ServiceCenter?> GetWithDetailsAsync(int id);   // includes ProviderServices + Slots
    }
}