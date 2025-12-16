using CarWare.Application.Common;
using CarWare.Application.DTOs.Vehicle;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<Result<List<VehicleDTOs>>> GetAllVehiclesAsync();
        Task<Result<List<VehicleDTOs>>> GetMyVehiclesAsync(string userId);
        Task<Result<List<BrandDTO>>> GetAllBrandsAsync();
        Task<Result<List<ModelDTO>>> GetModelsByBrandsAsync(int brandId);
        Task<Result<VehicleDTOs>> GetVehicleByIdAsync(int id);
        Task<Result<VehicleDTOs>> AddVehicleAsync(VehicleCreateDTO dto, string UserId);
        Task<Result<bool>> UpdateVehicleAsync(VehicleUpdateDTO dto, string userId);
        Task<Result<bool>> DeleteVehicleAsync(int id, string userId);
    }
}