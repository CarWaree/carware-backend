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
        Task<bool> UpdateVehicleAsync(VehicleDTOs dto);
        Task<bool> DeleteVehicleAsync(int id);
    }
}
