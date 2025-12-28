using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.maintenanceReminder;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
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

        public MaintenanceReminderService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> GetAllAsync()
        {
            var entities = await _uow.MaintenanceRepository.GetAllWithDetailsAsync();

            if (!entities.Any())
                return Result<IEnumerable<MaintenanceReminderResponseDto>>
                    .Fail("No maintenance reminders found.");

            var dtos = _mapper.Map<IEnumerable<MaintenanceReminderResponseDto>>(entities);
            return Result<IEnumerable<MaintenanceReminderResponseDto>>.Ok(dtos);
        }

        public async Task<Result<MaintenanceReminderResponseDto>> GetByIdAsync(int id, string userId)
        {
            var entity = await _uow.MaintenanceRepository.GetByIdWithDetailsAsync(id);
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

            await _uow.MaintenanceRepository.AddAsync(entity);
            await _uow.CompleteAsync();

            var entityWithDetails = await _uow.MaintenanceRepository.GetByIdWithDetailsAsync(entity.Id);
            var resultDto = _mapper.Map<MaintenanceReminderResponseDto>(entityWithDetails);

            return Result<MaintenanceReminderResponseDto>.Ok(resultDto);
        }

        public async Task<Result<MaintenanceReminderResponseDto>> UpdateAsync(UpdateMaintenanceReminderDto dto, string userId)
        {
            var existing = await _uow.MaintenanceRepository.GetByIdWithDetailsAsync(dto.Id);
            if (existing == null || existing.Vehicle.UserId != userId)
                return Result<MaintenanceReminderResponseDto>
                    .Fail($"MaintenanceReminder with id {dto.Id} not found or access denied.");

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            _uow.MaintenanceRepository.Update(existing);
            await _uow.CompleteAsync();

            var updatedEntity = await _uow.MaintenanceRepository.GetByIdWithDetailsAsync(existing.Id);
            var resultDto = _mapper.Map<MaintenanceReminderResponseDto>(updatedEntity);

            return Result<MaintenanceReminderResponseDto>.Ok(resultDto);
        }

        public async Task<Result<bool>> DeleteAsync(int id, string userId)
        {
            var existing = await _uow.MaintenanceRepository.GetByIdWithDetailsAsync(id);
            if (existing == null || existing.Vehicle.UserId != userId)
                return Result<bool>.Fail($"MaintenanceReminder with id {id} not found or access denied.");

            _uow.MaintenanceRepository.Delete(existing);
            await _uow.CompleteAsync();

            return Result<bool>.Ok(true);
        }

        public async Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> UpcomingMaintenanceAsync(string userId, int days = 7)
        {
            var today = DateTime.UtcNow.Date;
            var until = today.AddDays(days + 1).AddTicks(-1);

            var reminders = await _uow.MaintenanceRepository
                .GetUpcomingQueryable(days)
                    .Where(m => m.Vehicle.UserId == userId)
                    .Include(m => m.Vehicle)
                    .Include(m => m.Type)
                    .ToListAsync();

            if (!reminders.Any())
                return Result<IEnumerable<MaintenanceReminderResponseDto>>
                    .Fail("No upcoming maintenance reminders");

            var dtos = _mapper.Map<IEnumerable<MaintenanceReminderResponseDto>>(reminders);

            return Result<IEnumerable<MaintenanceReminderResponseDto>>.Ok(dtos);
        }

        public async Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> GetAllByCarAsync(int vehicleId, string userId)
        {
            var vehicle = await _uow.Repository<Vehicle>()
                .GetByIdAsync(vehicleId);

            if (vehicle == null || vehicle.UserId != userId)
                return Result<IEnumerable<MaintenanceReminderResponseDto>>
                    .Fail("Vehicle not found or access denied.");

            var list = await _uow.MaintenanceRepository.GetByVehicleWithDetailsAsync(vehicleId);

            if (!list.Any())
                return Result<IEnumerable<MaintenanceReminderResponseDto>>
                    .Fail("No maintenance reminders found for this vehicle.");

            var dtos = _mapper.Map<IEnumerable<MaintenanceReminderResponseDto>>(list);

            return Result<IEnumerable<MaintenanceReminderResponseDto>>.Ok(dtos);
        }
    }
}