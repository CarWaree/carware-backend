using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Repositories
{
    public class DeviceTokenRepository : GenericRepository<DeviceToken>, IDeviceTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public DeviceTokenRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<string>> GetActiveTokensByUserIdAsync(string userId)
        {
            return await _context.Set<DeviceToken>()
                .Where(t => t.UserId == userId && t.IsActive)
                .Select(t => t.Token)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllActiveUserIdsAsync()
        {
            return await _context.Set<DeviceToken>()
                .Where(t => t.IsActive)
                .Select(t => t.UserId)
                .Distinct()
                .ToListAsync();
        }
        public async Task<DeviceToken?> GetByTokenAsync(string token)
        {
            return await _context.Set<DeviceToken>()
                .FirstOrDefaultAsync(t => t.Token == token);
        }
    }
}