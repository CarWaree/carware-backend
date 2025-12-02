using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        Task<List<Vehicle>> GetAllCarsWithDetailsAsync();
        Task<Vehicle?> GetCarByIdWithDetailsAsync(int id);
        Task<List<Vehicle>> GetAllVehiclesAsync();
        Task<List<Model>> GetModelsByBrandAsync(int brandId);
    }
}
