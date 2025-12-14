using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
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

        public VehicleService(IUnitOfWork unitOfWork, IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        // Get All Vehicles (Admin)
        public async Task<Result<List<VehicleDTOs>>> GetAllVehiclesAsync()
        {
            var vehicles = await _unitOfWork.VehicleRepository.GetAllVehiclesAsync();
            if(vehicles == null || !vehicles.Any())
                return Result<List<VehicleDTOs>>.Fail("No vehicles found.");

            var dto = _mapper.Map<List<VehicleDTOs>>(vehicles);
            return Result<List<VehicleDTOs>>.Ok(dto);
        }

        // Get My Vehicles 
        public async Task<Result<List<VehicleDTOs>>> GetMyVehiclesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var vehicles = await _unitOfWork.VehicleRepository
                .GetVehiclesByUserIdAsync(userId);

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

        //Get all models for a specific brand
        public async Task<Result<List<ModelDTO>>> GetModelsByBrandsAsync(int brandId)
        {
            var models = await _unitOfWork.VehicleRepository.GetModelsByBrandAsync(brandId);

            var mapped = _mapper.Map<List<ModelDTO>>(models);

            return Result<List<ModelDTO>>.Ok(mapped);
        }

        // Get vehicle by ID (User)
        public async Task<Result<VehicleDTOs>> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetCarByIdWithDetailsAsync(id);

            if (vehicle == null)
                return Result<VehicleDTOs>.Fail("Vehicle not found");

            var dto = _mapper.Map<VehicleDTOs>(vehicle);
            return Result<VehicleDTOs>.Ok(dto);
        }

        // Add vehicle
        public async Task<Result<VehicleDTOs>> AddVehicleAsync(VehicleCreateDTO dto, string UserId)
        {
            var brand = (await _unitOfWork.Repository<Brand>()
                .FindAsync(b => b.Id == dto.BrandId)).FirstOrDefault();

            var model = (await _unitOfWork.Repository<Model>()
                .FindAsync(m => m.Id == dto.ModelId)).FirstOrDefault();

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
                return Result<VehicleDTOs>.Fail("User not found");

            if (brand == null || model == null)
                return Result<VehicleDTOs>.Fail("Invalid Brand, Model.");

            var existingVehicle = (await _unitOfWork.Repository<Vehicle>()
                        .FindAsync(v => v.BrandId == dto.BrandId
                       && v.ModelId == dto.ModelId
                       && v.Year == dto.Year
                       && v.Color == dto.Color
                       && v.UserId == UserId))
                        .FirstOrDefault();

            if (existingVehicle != null)
                return Result<VehicleDTOs>.Fail("This vehicle already exists for the user.");

            var vehicle = new Vehicle
            {
                Name = $"{brand.Name} {model.Name}",
                BrandId = dto.BrandId,
                ModelId = dto.ModelId,
                Year = dto.Year,
                Color = dto.Color,
                UserId = UserId
            };

            await _unitOfWork.Repository<Vehicle>().AddAsync(vehicle);
            await _unitOfWork.CompleteAsync();

            var mapper = _mapper.Map<VehicleDTOs>(vehicle);

            return Result<VehicleDTOs>.Ok(mapper);
        }

        // Update vehicle
        public async Task<Result<bool>> UpdateVehicleAsync(VehicleUpdateDTO dto, string userId)
        {
            var vehicle = (await _unitOfWork.Repository<Vehicle>()
                .FindAsync(v => v.Id == dto.Id && v.UserId == userId))
                .FirstOrDefault();

            if (vehicle == null)
                return Result<bool>.Fail("Vehicle not found or access denied.");

            // ✅ Check Brand only if changed
            if (dto.BrandId.HasValue)
            {
                var brandExists = await _unitOfWork.Repository<Brand>()
                    .AnyAsync(b => b.Id == dto.BrandId.Value);

                if (!brandExists)
                    return Result<bool>.Fail("Brand not found.");

                vehicle.BrandId = dto.BrandId.Value;
            }

            // Check Model only if changed
            if (dto.ModelId.HasValue)
            {
                var modelExists = await _unitOfWork.Repository<Model>()
                    .AnyAsync(m => m.Id == dto.ModelId.Value);

                if (!modelExists)
                    return Result<bool>.Fail("Model not found.");

                vehicle.ModelId = dto.ModelId.Value;
            }

            _mapper.Map(dto, vehicle);

            await _unitOfWork.CompleteAsync();
            return Result<bool>.Ok(true);
        }

        // Delete vehicle
        public async Task<Result<bool>> DeleteVehicleAsync(int id, string userId)
        {
            var vehicle = (await _unitOfWork.Repository<Vehicle>()
                .FindAsync(v => v.Id == id && v.UserId == userId))
                .FirstOrDefault();

            if (vehicle == null)
                return Result<bool>.Fail("Vehicle not found or access denied.");

            _unitOfWork.Repository<Vehicle>().Delete(vehicle);
            await _unitOfWork.CompleteAsync();

            return Result<bool>.Ok(true);
        }
    }
}