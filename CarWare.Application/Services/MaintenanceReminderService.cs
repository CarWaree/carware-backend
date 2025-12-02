using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.maintenanceReminder;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class MaintenanceReminderService : IMaintenanceReminderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<MaintenanceReminder> _repo;

        public MaintenanceReminderService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _repo = _uow.Repository<MaintenanceReminder>();
        }
        public async Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<MaintenanceReminderResponseDto>>(entities);
            return Result<IEnumerable<MaintenanceReminderResponseDto>>.Ok(dtos);
        }

        public async Task<Result<MaintenanceReminderResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return Result<MaintenanceReminderResponseDto>.Fail($"MaintenanceReminder with id {id} not found");

            var dto = _mapper.Map<MaintenanceReminderResponseDto>(entity);
            return Result<MaintenanceReminderResponseDto>.Ok(dto);
        }

        public async Task<Result<MaintenanceReminderResponseDto>> AddAsync(CreateMaintenanceReminderDto dto)
        {
            var entity = _mapper.Map<MaintenanceReminder>(dto);
            entity.CreatedAt = DateTime.UtcNow;

            await _repo.AddAsync(entity);
            await _uow.CompleteAsync();

            var resultDto = _mapper.Map<MaintenanceReminderResponseDto>(entity);
            return Result<MaintenanceReminderResponseDto>.Ok(resultDto);
        }

        public async Task<Result<MaintenanceReminderResponseDto>> UpdateAsync(UpdateMaintenanceReminderDto dto)
        {
            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null) return Result<MaintenanceReminderResponseDto>.Fail($"MaintenanceReminder with id {dto.Id} not found");

            _mapper.Map(dto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            _repo.Update(existing);
            await _uow.CompleteAsync();

            var resultDto = _mapper.Map<MaintenanceReminderResponseDto>(existing);
            return Result<MaintenanceReminderResponseDto>.Ok(resultDto);
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return Result<bool>.Fail($"MaintenanceReminder with id {id} not found");

            _repo.Delete(existing);
            await _uow.CompleteAsync();

            return Result<bool>.Ok(true);
        }

        public async Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> UpcomingMaintenanceAsync(int days = 7)
        {
            var now = DateTime.UtcNow;
            var until = now.AddDays(days);

            var list = await _repo.FindAsync(m => m.NextDueDate >= now && m.NextDueDate <= until);
            var dtos = _mapper.Map<IEnumerable<MaintenanceReminderResponseDto>>(list);

            return Result<IEnumerable<MaintenanceReminderResponseDto>>.Ok(dtos);
        }

        public async Task<Result<IEnumerable<MaintenanceReminderResponseDto>>> GetAllByCarAsync(int vehicleId)
        {
            var list = await _repo.FindAsync(m => m.VehicleId == vehicleId);
            var dtos = _mapper.Map<IEnumerable<MaintenanceReminderResponseDto>>(list);

            return Result<IEnumerable<MaintenanceReminderResponseDto>>.Ok(dtos);
        }
    }
}

