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

            await CreateServiceRequestFromAppointment(appointment);

            var createAppointment = await _unitOfWork.AppointmentRepository.GetByIdWithDetailsAsync(appointment.Id);
            var resultDto = _mapper.Map<AppointmentDto>(createAppointment);

            return Result<AppointmentDto>.Ok(resultDto);
        }

        #region Helper
        private async Task CreateServiceRequestFromAppointment(Appointment appointment)
        {
            var exists = await _unitOfWork.ServiceRequestRepository
                .GetAllQueryable()
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
                TotalPrice = 0,

                ServiceRequestServices = new List<ServiceRequestItem>
                {
                new ServiceRequestItem
                {
                    MaintenanceTypeId = appointment.ServiceId,
                    Description = appointment.Service?.Name
                }
                }
            };

            await _unitOfWork.ServiceRequestRepository.AddAsync(serviceRequest);
            await _unitOfWork.CompleteAsync();
        }
        #endregion
    }
}