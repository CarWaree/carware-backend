using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IDeviceTokenRepository : IGenericRepository<DeviceToken>
    {
        Task<List<string>> GetActiveTokensByUserIdAsync(string userId);
        Task<List<string>> GetAllActiveUserIdsAsync();
        Task<DeviceToken?> GetByTokenAsync(string token);
    }
}