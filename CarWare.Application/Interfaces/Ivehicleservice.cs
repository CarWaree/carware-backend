using CarWare.Application.DTOs.Vehicle;
using CarWare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IVehicleService
    {
        IQueryable<Vehicle> QueryVehicles();
        Task<VehicleDTOs> GetVehicleByIdAsync(int id);
        Task<VehicleDTOs> AddVehicleAsync(VehicleDTOs dto);
        Task<bool> UpdateVehicleAsync(VehicleDTOs dto);
        Task<bool> DeleteVehicleAsync(int id);
    }
}
