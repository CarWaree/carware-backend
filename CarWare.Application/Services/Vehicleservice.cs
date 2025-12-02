using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class Vehicleservice : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public Vehicleservice(IUnitOfWork unitOfWork, IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        // Get All Vehicles
        public async Task<Result<List<VehicleDTOs>>> GetAllVehiclesAsync()
        {
            var vehicles = await _unitOfWork.VehicleRepository.GetAllVehiclesAsync();
            if(vehicles == null || !vehicles.Any())
                return Result<List<VehicleDTOs>>.Fail("No vehicles found.");

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

        // Get vehicle by ID
        public async Task<Result<VehicleDTOs>> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetCarByIdWithDetailsAsync(id);

            if (vehicle == null)
                return Result<VehicleDTOs>.Fail("Vehicle not found");

            var dto = _mapper.Map<VehicleDTOs>(vehicle);
            return Result<VehicleDTOs>.Ok(dto);
        }

        // Add new vehicle
        public async Task<Result<VehicleDTOs>> AddVehicleAsync(VehicleCreateDTO dto)
        {
            var brand = (await _unitOfWork.Repository<Brand>()
                .FindAsync(b => b.Id == dto.BrandId)).FirstOrDefault();

            var model = (await _unitOfWork.Repository<Model>()
                .FindAsync(m => m.Id == dto.ModelId)).FirstOrDefault();

            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (brand == null || model == null || user == null)
                return Result<VehicleDTOs>.Fail("Invalid Brand, Model, or User.");

            var existingVehicle = (await _unitOfWork.Repository<Vehicle>()
                        .FindAsync(v => v.BrandId == dto.BrandId
                       && v.ModelId == dto.ModelId
                       && v.Year == dto.Year
                       && v.Color == dto.Color
                       && v.UserId == dto.UserId))
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
                UserId = dto.UserId
            };

            await _unitOfWork.Repository<Vehicle>().AddAsync(vehicle);
            await _unitOfWork.CompleteAsync();

            var mapper = _mapper.Map<VehicleDTOs>(vehicle);

            return Result<VehicleDTOs>.Ok(mapper);
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