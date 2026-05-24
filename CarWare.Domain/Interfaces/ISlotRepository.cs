using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface ISlotRepository : IGenericRepository<Slot>
    {
        Task AddRangeAsync(List<Slot> slots);
        Task<List<Slot>> GetByCenterIdAsync(int centerId);
        Task DeleteByCenterIdAsync(int centerId);
    }
}