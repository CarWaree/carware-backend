using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class Vehicleservice : IVehicleService
    {

        private readonly IUnitOfWork _unitOfWork;

        public Vehicleservice(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            var repo = _unitOfWork.Repository<Vehicle>();

            var vehicle = await repo.GetByIdAsync(id);
            if (vehicle == null)
                return false;

            repo.Delete(vehicle);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> UpdateVehicleAsync(VehicleDTOs dto)
        {
            var repo = _unitOfWork.Repository<Vehicle>();

            var vehicle = await repo.GetByIdAsync(dto.Id);
            if (vehicle == null)
                return false;

            
            vehicle.Brand = dto.Brand;
            vehicle.Model = dto.Model;
            vehicle.Year = dto.Year;
            vehicle.Color = dto.Color;

            repo.Update(vehicle);
            await _unitOfWork.CompleteAsync();

            return true;
        }


    }
}
