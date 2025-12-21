using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ILogger<VehicleService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        // Get All Vehicles (Admin)
        public async Task<Result<List<VehicleDTOs>>> GetAllVehiclesAsync()
        {
            var vehicles = await _unitOfWork.VehicleRepository.GetAllVehiclesAsync();
            if (vehicles == null || !vehicles.Any())
                return Result<List<VehicleDTOs>>.Fail("No vehicles found.");

            var dto = _mapper.Map<List<VehicleDTOs>>(vehicles);
            return Result<List<VehicleDTOs>>.Ok(dto);
        }

        // Get My Vehicles
        public async Task<Result<List<VehicleDTOs>>> GetMyVehiclesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("GetMyVehiclesAsync: User not found with ID {UserId}", userId);
                return Result<List<VehicleDTOs>>.Fail("User not found.");
            }

            var vehicles = await _unitOfWork.VehicleRepository.GetVehiclesByUserIdAsync(userId);
            if (vehicles == null || !vehicles.Any())
                return Result<List<VehicleDTOs>>.Fail("You have no vehicles.");

            var dto = _mapper.Map<List<VehicleDTOs>>(vehicles);
            return Result<List<VehicleDTOs>>.Ok(dto);
        }

        // Get All Brands
        public async Task<Result<List<BrandDTO>>> GetAllBrandsAsync()
        {
            var brands = await _unitOfWork.Repository<Brand>().GetAllAsync();
            var mapped = _mapper.Map<List<BrandDTO>>(brands);
            return Result<List<BrandDTO>>.Ok(mapped);
        }

        // Get Models by Brand
        public async Task<Result<List<ModelDTO>>> GetModelsByBrandsAsync(int brandId)
        {
            var models = await _unitOfWork.VehicleRepository.GetModelsByBrandAsync(brandId);
            var mapped = _mapper.Map<List<ModelDTO>>(models);
            return Result<List<ModelDTO>>.Ok(mapped);
        }

        // Get Vehicle by ID
        public async Task<Result<VehicleDTOs>> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetCarByIdWithDetailsAsync(id);
            if (vehicle == null)
                return Result<VehicleDTOs>.Fail("Vehicle not found");

            var dto = _mapper.Map<VehicleDTOs>(vehicle);
            return Result<VehicleDTOs>.Ok(dto);
        }

        // Add Vehicle
        public async Task<Result<VehicleDTOs>> AddVehicleAsync(VehicleCreateDTO dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("AddVehicleAsync: User not found with ID {UserId}", userId);
                return Result<VehicleDTOs>.Fail("User not found");
            }

            var brand = (await _unitOfWork.Repository<Brand>().FindAsync(b => b.Id == dto.BrandId)).FirstOrDefault();
            var model = (await _unitOfWork.Repository<Model>().FindAsync(m => m.Id == dto.ModelId)).FirstOrDefault();

            if (brand == null || model == null)
            {
                _logger.LogWarning("AddVehicleAsync: Invalid Brand or Model. BrandId: {BrandId}, ModelId: {ModelId}", dto.BrandId, dto.ModelId);
                return Result<VehicleDTOs>.Fail("Invalid Brand or Model.");
            }

            var existingVehicle = (await _unitOfWork.VehicleRepository
                .FindAsync(v => v.BrandId == dto.BrandId &&
                                 v.ModelId == dto.ModelId &&
                                 v.Year == dto.Year &&
                                 v.Color == dto.Color &&
                                 v.UserId == userId))
                .FirstOrDefault();

            if (existingVehicle != null)
            {
                _logger.LogWarning("AddVehicleAsync: Vehicle already exists for user {UserId}. BrandId: {BrandId}, ModelId: {ModelId}", userId, dto.BrandId, dto.ModelId);
                return Result<VehicleDTOs>.Fail("This vehicle already exists for the user.");
            }

            var vehicle = new Vehicle
            {
                Name = $"{brand.Name} {model.Name}",
                BrandId = dto.BrandId,
                ModelId = dto.ModelId,
                Year = dto.Year,
                Color = dto.Color,
                UserId = userId
            };

            await _unitOfWork.VehicleRepository.AddAsync(vehicle);
            await _unitOfWork.CompleteAsync();

            var vehicleWithDetails = await _unitOfWork.VehicleRepository.GetCarByIdWithDetailsAsync(vehicle.Id);
            var vehicleDto = _mapper.Map<VehicleDTOs>(vehicleWithDetails);

            return Result<VehicleDTOs>.Ok(vehicleDto);
        }

        // Update Vehicle
        public async Task<Result<bool>> UpdateVehicleAsync(VehicleUpdateDTO dto, string userId)
        {
            var vehicle = (await _unitOfWork.VehicleRepository
                .FindAsync(v => v.Id == dto.Id && v.UserId == userId))
                .FirstOrDefault();

            if (vehicle == null)
                return Result<bool>.Fail("Vehicle not found or access denied.");

            if (dto.BrandId.HasValue)
            {
                var brandExists = await _unitOfWork.Repository<Brand>().AnyAsync(b => b.Id == dto.BrandId.Value);
                if (!brandExists)
                    return Result<bool>.Fail("Brand not found.");
                vehicle.BrandId = dto.BrandId.Value;
            }

            if (dto.ModelId.HasValue)
            {
                var modelExists = await _unitOfWork.Repository<Model>().AnyAsync(m => m.Id == dto.ModelId.Value);
                if (!modelExists)
                    return Result<bool>.Fail("Model not found.");
                vehicle.ModelId = dto.ModelId.Value;
            }

            _mapper.Map(dto, vehicle);
            await _unitOfWork.CompleteAsync();

            return Result<bool>.Ok(true);
        }

        // Delete Vehicle
        public async Task<Result<bool>> DeleteVehicleAsync(int id, string userId)
        {
            var vehicle = (await _unitOfWork.VehicleRepository
                .FindAsync(v => v.Id == id && v.UserId == userId))
                .FirstOrDefault();

            if (vehicle == null)
                return Result<bool>.Fail("Vehicle not found or access denied.");

            _unitOfWork.VehicleRepository.Delete(vehicle);
            await _unitOfWork.CompleteAsync();

            return Result<bool>.Ok(true);
        }
    }
}