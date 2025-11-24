using CarWare.Application.DTOs.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDTOs>> GetAllVehiclesAsync();

        // Get a vehicle by ID
        Task<VehicleDTOs> GetVehicleByIdAsync(int id);

        // Add a new vehicle
        Task<VehicleDTOs> AddVehicleAsync(VehicleDTOs dto);

        // Update a vehicle
        Task<bool> UpdateVehicleAsync(VehicleDTOs dto);

        // Delete a vehicle
        Task<bool> DeleteVehicleAsync(int id);
    }
}
