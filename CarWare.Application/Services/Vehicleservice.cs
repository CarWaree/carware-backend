using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Infrastructure.Repositories;
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

        public Vehicleservice(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Get All Vehicles
        public async Task<Result<List<VehicleDTOs>>> GetAllVehiclesAsync()
        {
            var vehicls = await _unitOfWork.VehicleRepository.GetAllCarsWithDetailsAsync();
            var dto = _mapper.Map<List<VehicleDTOs>>(vehicls);
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
            try
            {
                // Get brand & model names
                var brand = await _unitOfWork.VehicleRepository.GetByIdAsync(dto.BrandId);
                var model = await _unitOfWork.VehicleRepository.GetByIdAsync(dto.ModelId);

                if (brand == null || model == null)
                    return Result<VehicleDTOs>.Fail("Invalid brand or model");

                // Build Vehicle object
                var car = new Vehicle
                {
                    BrandId = dto.BrandId,
                    ModelId = dto.ModelId,
                    Year = dto.Year,
                    Color = dto.Color,
                    UserId = "f31e859f-7e38-438a-9c0e-2db7b9086a66",           // JWT later 
                    Name = $"{brand.Name} {model.Name}" // NOT NULL
                };

                // Save in DB
                await _unitOfWork.VehicleRepository.AddAsync(car);
                await _unitOfWork.CompleteAsync();

                // Fetch with relations
                var carWithDetails = await _unitOfWork.VehicleRepository.GetCarByIdWithDetailsAsync(car.Id);
                var mapped = _mapper.Map<VehicleDTOs>(carWithDetails);

                return Result<VehicleDTOs>.Ok(mapped);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("Inner Exception: " + ex.InnerException?.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                throw;
            }
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