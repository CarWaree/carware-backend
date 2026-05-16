using CarWare.Application.Common;
using CarWare.Application.DTOs.Notification;
using CarWare.Application.DTOs.ServiceRequests;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CarWare.Application.Services.ServiceRequests
{
    public class ServiceRequestWorkflowService
        : IServiceRequestWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ICurrentUserService _currentUserService;

        public ServiceRequestWorkflowService(
            IUnitOfWork unitOfWork,
            IBackgroundJobClient backgroundJobClient,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _backgroundJobClient = backgroundJobClient;
            _currentUserService = currentUserService;
        }

        private int CenterId =>
            _currentUserService.ServiceCenterId
            ?? throw new Exception("ServiceCenterId not found");

        public async Task<Result<AcceptResponseDto>> AcceptAsync(int id, AcceptServiceRequestDto dto)
        {
            var request = await _unitOfWork.ServiceRequestRepository
                .GetAllQueryable()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ServiceCenterId == CenterId);

            if (request == null)
                return Result<AcceptResponseDto>
                    .Fail("Service Request not found");

            if (request.ServiceStatus != ServiceRequestStatus.Pending)
                return Result<AcceptResponseDto>
                    .Fail("Only pending requests can be accepted");

            request.ServiceStatus = ServiceRequestStatus.Accepted;
            request.AcceptedAt = DateTime.UtcNow;
            request.EstimatedCost = dto.EstimatedCost;
            request.EstimatedCompletion = dto.EstimatedCompletion;
            request.TechnicianId = dto.TechnicianId;

            _unitOfWork.ServiceRequestRepository.Update(request);

            await _unitOfWork.CompleteAsync();

            var technicianName = request.Technician?.UserName;

            var body =
                $"Your request has been accepted by {request.Technician?.UserName}.\n" +
                $"Estimated Cost: {request.EstimatedCost}\n" +
                $"Estimated Completion: {request.EstimatedCompletion:yyyy-MM-dd HH:mm}";

            _backgroundJobClient.Enqueue<NotificationJobs>(job =>
                job.Send(new SendNotificationDto
                {
                    UserId = request.UserId,
                    Title = "Request Confirmed",
                    Body = body,
                    Channel = NotificationChannel.Push
                }));

            return Result<AcceptResponseDto>.Ok(
                new AcceptResponseDto
                {
                    Id = request.Id,
                    Status = request.ServiceStatus,
                    AcceptedAt = request.AcceptedAt!.Value
                });
        }

        public async Task<Result<RejectResponseDto>>
            RejectAsync(int id, RejectServiceRequestDto dto)
        {
            var request = await _unitOfWork.ServiceRequestRepository
                .GetAllQueryable()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ServiceCenterId == CenterId);

            if (request == null)
                return Result<RejectResponseDto>
                    .Fail("Service Request not found");

            if (request.ServiceStatus != ServiceRequestStatus.Pending)
                return Result<RejectResponseDto>
                    .Fail("Only pending requests can be rejected");

            request.ServiceStatus = ServiceRequestStatus.Rejected;
            request.RejectedAt = DateTime.UtcNow;
            request.RejectionReason = dto.RejectionReason;

            _unitOfWork.ServiceRequestRepository.Update(request);

            await _unitOfWork.CompleteAsync();

            return Result<RejectResponseDto>.Ok(
                new RejectResponseDto
                {
                    Id = request.Id,
                    Status = request.ServiceStatus,
                    RejectedAt = request.RejectedAt!.Value
                });
        }

        public async Task<Result<CompleteResponseDto>>
            CompleteAsync(int id, CompleteServiceRequestDto dto)
        {
            var request = await _unitOfWork.ServiceRequestRepository
                .GetAllQueryable()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ServiceCenterId == CenterId);

            if (request == null)
                return Result<CompleteResponseDto>
                    .Fail("Service Request not found");

            if (request.ServiceStatus != ServiceRequestStatus.Accepted)
                return Result<CompleteResponseDto>
                    .Fail("Only accepted requests can be completed");

            request.ServiceStatus = ServiceRequestStatus.Completed;
            request.CompletedAt = DateTime.UtcNow;
            request.TotalPrice = dto.TotalPrice;
            request.TechnicianNotes = dto.TechnicianNotes;

            _unitOfWork.ServiceRequestRepository.Update(request);

            await _unitOfWork.CompleteAsync();

            return Result<CompleteResponseDto>.Ok(
                new CompleteResponseDto
                {
                    Id = request.Id,
                    Status = request.ServiceStatus,
                    CompletedAt = request.CompletedAt!.Value
                });
        }
    }
}