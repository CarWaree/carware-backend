using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.maintenanceReminder;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class MaintenanceReminderService : IMaintenanceReminderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMaintenanceRepository _repo;

        public MaintenanceReminderService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _repo = _uow.MaintenanceRepository;
        }
        public async Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> GetAllAsync()
        {
            var entities = await _repo.GetAllWithDetailsAsync();

            if (!entities.Any())
                return Result<IEnumerable<MaintenanceReminderResponseDto>>
                    .Fail("No maintenance reminders found.");

            var dtos = _mapper.Map<IEnumerable<MaintenanceReminderResponseDto>>(entities);
            return Result<IEnumerable<MaintenanceReminderResponseDto>>.Ok(dtos);
        }

        public async Task<Result<MaintenanceReminderResponseDto>> GetByIdAsync(int id, string userId)
        {
            var entity = await _repo.GetByIdWithDetailsAsync(id);
            if (entity == null || entity.Vehicle.UserId != userId)
                return Result<MaintenanceReminderResponseDto>.Fail($"MaintenanceReminder with id {id} not found or access denied.");

            var dto = _mapper.Map<MaintenanceReminderResponseDto>(entity);
            return Result<MaintenanceReminderResponseDto>.Ok(dto);
        }

        public async Task<Result<MaintenanceReminderResponseDto>> AddAsync(CreateMaintenanceReminderDto dto, string userId)
        {
            var vehicle = await _uow.Repository<Vehicle>()
                .GetByIdAsync(dto.VehicleId);

            if (vehicle == null || vehicle.UserId != userId)
                return Result<MaintenanceReminderResponseDto>
                    .Fail("Invalid vehicle or access denied.");

            var entity = _mapper.Map<MaintenanceReminder>(dto);
            entity.CreatedAt = DateTime.UtcNow;

            await _repo.AddAsync(entity);
            await _uow.CompleteAsync();

            var entityWithDetails = await _repo.GetByIdWithDetailsAsync(entity.Id);
            var resultDto = _mapper.Map<MaintenanceReminderResponseDto>(entityWithDetails);

            return Result<MaintenanceReminderResponseDto>.Ok(resultDto);
        }

        public async Task<Result<MaintenanceReminderResponseDto>> UpdateAsync(UpdateMaintenanceReminderDto dto, string userId)
        {
            var existing = await _repo.GetByIdWithDetailsAsync(dto.Id);
            if (existing == null || existing.Vehicle.UserId != userId)
                return Result<MaintenanceReminderResponseDto>
                    .Fail($"MaintenanceReminder with id {dto.Id} not found or access denied.");

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            _repo.Update(existing);
            await _uow.CompleteAsync();

            var updatedEntity = await _repo.GetByIdWithDetailsAsync(existing.Id);
            var resultDto = _mapper.Map<MaintenanceReminderResponseDto>(updatedEntity);

            return Result<MaintenanceReminderResponseDto>.Ok(resultDto);
        }

        public async Task<Result<bool>> DeleteAsync(int id, string userId)
        {
            var existing = await _repo.GetByIdWithDetailsAsync(id);
            if (existing == null || existing.Vehicle.UserId != userId)
                return Result<bool>.Fail($"MaintenanceReminder with id {id} not found or access denied.");

            _repo.Delete(existing);
            await _uow.CompleteAsync();

            return Result<bool>.Ok(true);
        }

        public async Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> UpcomingMaintenanceAsync(string userId, int days = 7)
        {
            var now = DateTime.UtcNow;
            var until = now.AddDays(days);

            var list = await _repo.Query()
                .Include(m => m.Vehicle)
                .Include(m => m.Type)
                .Where(m =>
                    m.NextDueDate >= now &&
                    m.NextDueDate <= until &&
                    m.Vehicle.UserId == userId)
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<MaintenanceReminderResponseDto>>(list);

            return Result<IEnumerable<MaintenanceReminderResponseDto>>.Ok(dtos);
        }

        public async Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> GetAllByCarAsync(int vehicleId, string userId)
        {
            var vehicle = await _uow.Repository<Vehicle>()
                .GetByIdAsync(vehicleId);

            if (vehicle == null || vehicle.UserId != userId)
                return Result<IEnumerable<MaintenanceReminderResponseDto>>
                    .Fail("Vehicle not found or access denied.");

            var list = await _repo.FindAsync(m => m.VehicleId == vehicleId);
            var dtos = _mapper.Map<IEnumerable<MaintenanceReminderResponseDto>>(list);

            return Result<IEnumerable<MaintenanceReminderResponseDto>>.Ok(dtos);
        }
    }
}

