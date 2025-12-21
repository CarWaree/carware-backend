using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Appointment;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AppointmentService
            (UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<AppointmentDto>> CancelAsync(int id, string userId)
        {
            var repo = _unitOfWork.Repository<Appointment>();
            var appointment = await repo.GetByIdAsync(id);

            if (appointment == null || appointment.UserId != userId)
                return Result<AppointmentDto>
                    .Fail($"Appointment with id {id} not found or access denied.");

            if (appointment.Status != AppointmentStatus.Pending)
                return Result<AppointmentDto>
                    .Fail("Only pending appointments can be cancelled.");

            appointment.Status = AppointmentStatus.Cancelled;
            repo.Update(appointment);

            await _unitOfWork.CompleteAsync();

            var dto = _mapper.Map<AppointmentDto>(appointment);
            return Result<AppointmentDto>.Ok(dto);
        }

        public async Task<Result<AppointmentDto>> UpdateStatusAsync(int id, AppointmentStatus status)
        {
            var repo = _unitOfWork.Repository<Appointment>();
            var appointment = await repo.GetByIdAsync(id);

            if (appointment == null)
                return Result<AppointmentDto>
                    .Fail($"Appointment with id {id} not found.");

            if (appointment.Status == AppointmentStatus.Cancelled)
                return Result<AppointmentDto>
                    .Fail("Cancelled appointment cannot be updated.");

            appointment.Status = status;
            repo.Update(appointment);

            await _unitOfWork.CompleteAsync();

            var dto = _mapper.Map<AppointmentDto>(appointment);
            return Result<AppointmentDto>.Ok(dto);
        }

        public async Task<Result<List<AppointmentDto>>> GetUserAppointmentsAsync(string userId)
        {
            var appointments = await _unitOfWork.AppointmentRepository.GetUserAppointmentsAsync(userId);

            var result = _mapper.Map<List<AppointmentDto>>(appointments);

            return Result<List<AppointmentDto>>.Ok(result);
        }

        public async Task<Result<AppointmentDto>> AddAppointmentAsync(CreateAppointmentDto dto, string userId)
        {
            //validation
            if (dto == null)
                return Result<AppointmentDto>.Fail("Appointment data is required");

            if (dto.Date < DateTime.UtcNow)
                return Result<AppointmentDto>.Fail("Appointment data cannot be in the past");

            //vehicle owner
            var vehicle =  await _unitOfWork.Repository<Vehicle>().GetByIdAsync(dto.VehicleId);

            if (vehicle == null || vehicle.UserId != userId)
                return Result<AppointmentDto>.Fail("Invalid Vehicle");

            //service provider exists 
            if (!await _unitOfWork.Repository<ServiceCenter>()
                    .AnyAsync(sc => sc.Id == dto.ServiceCenterId))
                return Result<AppointmentDto>.Fail("Service center not found");

            //time slot conflict 
            bool slotTaken = await _unitOfWork.Repository<Appointment>()
                .AnyAsync(a =>
                    a.ServiceCenterId == dto.ServiceCenterId &&
                    a.Date.Date == dto.Date.Date &&
                    a.TimeSlot == dto.TimeSlot &&
                    a.Status != AppointmentStatus.Cancelled);

            //mapping 
            var appointment =  _mapper.Map<Appointment>(dto);
            appointment.UserId = userId;
            appointment.Status = AppointmentStatus.Pending;

            //add to repo
            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);

            //save 
            await _unitOfWork.CompleteAsync();
            var resultDto = _mapper.Map<AppointmentDto>(appointment);

            return Result<AppointmentDto>.Ok(resultDto);
        }
    }
}