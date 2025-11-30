using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        //Task<List<string>> GetAllBrandsAsync();
        //Task<List<Vehicle>> GetModelsByBrandAsync(string brandName);
    }
}
