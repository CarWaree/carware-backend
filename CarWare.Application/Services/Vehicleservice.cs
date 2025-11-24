using AutoMapper;
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

        // Get all vehicles
        public IQueryable<Vehicle> QueryVehicles()
        {
            return _unitOfWork.Repository<Vehicle>().Query();
        }

        // Get vehicle by ID
        public async Task<VehicleDTOs> GetVehicleByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<Vehicle>();
            var vehicle = await repo.GetByIdAsync(id);
            if (vehicle == null) return null;

            return _mapper.Map<VehicleDTOs>(vehicle);
        }

        // Add new vehicle
        public async Task<VehicleDTOs> AddVehicleAsync(VehicleDTOs dto)
        {
            var repo = _unitOfWork.Repository<Vehicle>();
            var vehicle = _mapper.Map<Vehicle>(dto);

            await repo.AddAsync(vehicle);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<VehicleDTOs>(vehicle);
        }

        // Update vehicle
        public async Task<bool> UpdateVehicleAsync(VehicleDTOs dto)
        {
            var repo = _unitOfWork.Repository<Vehicle>();
            var vehicle = await repo.GetByIdAsync(dto.Id);
            if (vehicle == null) return false;

            _mapper.Map(dto, vehicle);

            repo.Update(vehicle);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        // Delete vehicle
        public async Task<bool> DeleteVehicleAsync(int id)
        {
            var repo = _unitOfWork.Repository<Vehicle>();
            var vehicle = await repo.GetByIdAsync(id);
            if (vehicle == null) return false;

            repo.Delete(vehicle);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}