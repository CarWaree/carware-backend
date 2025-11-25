using CarWare.Application.Common;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IVehicleService
    {
        IQueryable<Vehicle> QueryVehicles();

        Task<Result<VehicleDTOs>> GetVehicleByIdAsync(int id);
        Task<Result<VehicleDTOs>> AddVehicleAsync(VehicleDTOs dto);
        Task<Result<bool>> UpdateVehicleAsync(VehicleDTOs dto);
        Task<Result<bool>> DeleteVehicleAsync(int id);
    }
}
