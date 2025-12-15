using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Appointment;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        // PUT /api/appointments/{id}/cancel
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

        // PUT /api/appointments/{id}/status

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
    }
}
