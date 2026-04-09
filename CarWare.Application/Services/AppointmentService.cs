using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Appointment;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private async Task CreateServiceRequestFromAppointment(Appointment appointment)
        {
            bool exists = await _unitOfWork.Repository<ServiceRequest>()
                .AnyAsync(x => x.AppointmentId == appointment.Id);

            if (exists)
                return;

            var serviceRequest = new ServiceRequest
            {
                UserId = appointment.UserId,
                VehicleId = appointment.VehicleId,
                AppointmentId = appointment.Id,
                ServiceCenterId = appointment.ServiceCenterId,
                ServiceDate = appointment.Date,
                ServiceStatus = ServiceRequestStatus.Pending,
                PaymentMethod = PaymentMethod.Cash,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0
            };

            await _unitOfWork.Repository<ServiceRequest>()
                .AddAsync(serviceRequest);

            await _unitOfWork.CompleteAsync();

            var serviceRequestService = new ServiceRequestService
            {
                ServiceRequestId = serviceRequest.Id,
                MaintenanceTypeId = appointment.ServiceId, 
                Description = appointment.Service?.Name
            };

            await _unitOfWork.Repository<ServiceRequestService>()
                .AddAsync(serviceRequestService);
            await _unitOfWork.CompleteAsync();
        }

        private static bool IsValidTransition(AppointmentStatus current, AppointmentStatus next)
        {
            if (current == AppointmentStatus.Pending)
                return next == AppointmentStatus.Confirmed || next == AppointmentStatus.Cancelled;

            if (current == AppointmentStatus.Confirmed)
                return next == AppointmentStatus.Completed || next == AppointmentStatus.Cancelled;

            return false;
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

            var updated = await _unitOfWork.AppointmentRepository.GetByIdWithDetailsAsync(id);
            var dto = _mapper.Map<AppointmentDto>(updated);
            return Result<AppointmentDto>.Ok(dto);
        }

        public async Task<Result<AppointmentDto>> UpdateStatusAsync(int id, UpdateStatusDto statusDto)
        {
            var repo = _unitOfWork.Repository<Appointment>();
            var appointment = await repo.GetByIdAsync(id);

            if (appointment == null)
                return Result<AppointmentDto>
                    .Fail($"Appointment with id {id} not found.");

            if (appointment.Status == AppointmentStatus.Completed)
                return Result<AppointmentDto>.Fail("Cannot update a completed appointment.");

            if (appointment.Status == AppointmentStatus.Cancelled)
                return Result<AppointmentDto>.Fail("Cannot update a cancelled appointment.");

            if (!IsValidTransition(appointment.Status, statusDto.Status))
                return Result<AppointmentDto>.Fail(
                    $"Cannot move from {appointment.Status} to {statusDto.Status}.");

            appointment.Status = statusDto.Status;
            repo.Update(appointment);

            if (statusDto.Status == AppointmentStatus.Completed)
            {
                var withDetails = await _unitOfWork.AppointmentRepository.GetByIdWithDetailsAsync(id);
                withDetails.Status = AppointmentStatus.Completed;
                await CreateServiceRequestFromAppointment(withDetails);
            }
            else
            {
                await _unitOfWork.CompleteAsync();
            }

            var updated = await _unitOfWork.AppointmentRepository.GetByIdWithDetailsAsync(id);
            var dto = _mapper.Map<AppointmentDto>(updated);
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
            var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(dto.VehicleId);

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

            if (slotTaken)
                return Result<AppointmentDto>.Fail("This time slot is already booked.");

            //mapping 
            var appointment = _mapper.Map<Appointment>(dto);
            appointment.UserId = userId;
            appointment.Status = AppointmentStatus.Pending;

            //add to repo
            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);

            //save 
            await _unitOfWork.CompleteAsync();

            var createAppointment = await _unitOfWork.AppointmentRepository.GetByIdWithDetailsAsync(appointment.Id);
            var resultDto = _mapper.Map<AppointmentDto>(createAppointment);

            return Result<AppointmentDto>.Ok(resultDto);
        }
    }
}