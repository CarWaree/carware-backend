using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class Vehicleservice : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public Vehicleservice(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Query for pagination
        public IQueryable<Vehicle> QueryVehicles()
        {
            return _unitOfWork.Repository<Vehicle>().Query();
        }

        // Get vehicle by ID
        public async Task<Result<VehicleDTOs>> GetVehicleByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<Vehicle>();
            var vehicle = await repo.GetByIdAsync(id);

            if (vehicle == null)
                return Result<VehicleDTOs>.Fail("Vehicle not found");

            var dto = _mapper.Map<VehicleDTOs>(vehicle);
            return Result<VehicleDTOs>.Ok(dto);
        }

        // Add new vehicle
        public async Task<Result<VehicleDTOs>> AddVehicleAsync(VehicleDTOs dto)
        {
            var repo = _unitOfWork.Repository<Vehicle>();
            var vehicle = _mapper.Map<Vehicle>(dto);

            await repo.AddAsync(vehicle);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<VehicleDTOs>(vehicle);
            return Result<VehicleDTOs>.Ok(resultDto);
        }

        // Update vehicle
        public async Task<Result<bool>> UpdateVehicleAsync(VehicleDTOs dto)
        {
            var repo = _unitOfWork.Repository<Vehicle>();
            var vehicle = await repo.GetByIdAsync(dto.Id);

            if (vehicle == null)
                return Result<bool>.Fail("Vehicle not found");

            _mapper.Map(dto, vehicle);

            repo.Update(vehicle);
            await _unitOfWork.CompleteAsync();

            return Result<bool>.Ok(true);
        }

        // Delete vehicle
        public async Task<Result<bool>> DeleteVehicleAsync(int id)
        {
            var repo = _unitOfWork.Repository<Vehicle>();
            var vehicle = await repo.GetByIdAsync(id);

            if (vehicle == null)
                return Result<bool>.Fail("Vehicle not found");

            repo.Delete(vehicle);
            await _unitOfWork.CompleteAsync();

            return Result<bool>.Ok(true);
        }
    }
}
