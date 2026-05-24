using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Repositories
{
    public class SlotRepository
        : GenericRepository<Slot>, ISlotRepository
    {
        private readonly ApplicationDbContext _context;

        public SlotRepository(ApplicationDbContext context) : base(context)
            => _context = context;

        public async Task AddRangeAsync(List<Slot> slots)
            => await _context.Slots.AddRangeAsync(slots);

        // Active slots only
        public Task<List<Slot>> GetByCenterIdAsync(int centerId) =>
            _context.Slots
                    .Where(s => s.ServiceCenterId == centerId && s.IsActive)
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .ToListAsync();

        // Clears all slots for a center
        public async Task DeleteByCenterIdAsync(int centerId)
        {
            var existing = await _context.Slots
                .Where(s => s.ServiceCenterId == centerId)
                .ToListAsync();

            _context.Slots.RemoveRange(existing);
        }
    }
}