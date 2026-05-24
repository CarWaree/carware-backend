using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IProviderServicesRepository
    {
        Task AddRangeAsync(List<ProviderServices> providers);
        Task<List<ProviderServices>> GetByCenterIdAsync(int centerId);
        Task DeleteByCenterIdAsync(int centerId);
    }
}