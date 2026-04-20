using CarWare.Application.Common;
using CarWare.Application.DTOs.History;
using CarWare.Application.DTOs.ServiceRequests;
using CarWare.Application.helper;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IUnitOfWork _unitOfWork;

    public ServiceRequestService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    #region Dashboard
    public async Task<Result<ServiceRequestListResponse>> GetDashboardRequestsAsync
        (ServiceRequestQueryParams queryParams)
    {
        var query = _unitOfWork.ServiceRequestRepository
            .GetAllQueryable()
            .OrderByDescending(x => x.CreatedAt);

        // ── Counts ──
        var counts = new CountsDto
        {
            Pending = await query.CountAsync(x => x.ServiceStatus == ServiceRequestStatus.Pending),
            Active = await query.CountAsync(x => x.ServiceStatus == ServiceRequestStatus.Accepted),
            Completed = await query.CountAsync(x => x.ServiceStatus == ServiceRequestStatus.Completed)
        };

        // ── Filter ──
        if (!string.IsNullOrEmpty(queryParams.Status) && queryParams.Status != "all")
        {
            if (Enum.TryParse<ServiceRequestStatus>(queryParams.Status, true, out var statusEnum))
                query = (IOrderedQueryable<ServiceRequest>)query
                    .Where(x => x.ServiceStatus == statusEnum);
        }

        // ── Search ──
        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            var s = queryParams.Search.ToLower();
            query = (IOrderedQueryable<ServiceRequest>)query
                .Where(x => x.User.FullName.ToLower().Contains(s) ||
                            x.Vehicle.Brand.Name.ToLower().Contains(s));
        }

        // ── Pagination ──
        var total = await query.CountAsync();
        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.Limit)
            .Take(queryParams.Limit)
            .Select(x => new ServiceRequestDto
            {
                Id = x.Id,
                Status = x.ServiceStatus,
                CreatedAt = x.CreatedAt,
                ServiceType = x.ServiceRequestServices
                    .Select(s => s.MaintenanceType.Name)
                    .FirstOrDefault(),
                Car = new CarDto
                {
                    Brand = x.Vehicle.Brand.Name,
                    Model = x.Vehicle.Model.Name,
                    Color = x.Vehicle.Color
                },
                Client = new ClientDto
                {
                    Id = x.TechnicianId,
                    Name = x.User.FullName
                }
            })
            .ToListAsync();

        var result = new ServiceRequestListResponse
        {
            Total = total,
            Counts = counts,
            Data = data
        };

        return Result<ServiceRequestListResponse>.Ok(result);
    }
    public async Task<Result<ServiceRequestDto>> GetRequestDetailsAsync(int id)
    {
        var entity = await _unitOfWork.ServiceRequestRepository
            .GetAllQueryable()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return Result<ServiceRequestDto>.Fail("Service Request not found");

        var result = new ServiceRequestDto
        {
            Id = entity.Id,
            Status = entity.ServiceStatus,
            CreatedAt = entity.CreatedAt,
            ServiceType = entity.ServiceRequestServices
                .Select(x => x.MaintenanceType.Name)
                .FirstOrDefault(),

            Car = new CarDto
            {
                Brand = entity.Vehicle.Brand.Name,
                Model = entity.Vehicle.Model.Name,
                Color = entity.Vehicle.Color
            },

            Client = new ClientDto
            {
                Id = entity.TechnicianId,
                Name = entity.User.FullName
            }
        };

        return Result<ServiceRequestDto>.Ok(result);
    }
    #endregion

    #region Work Flow Actions 
    public async Task<Result<AcceptResponseDto>> AcceptAsync(int id, AcceptServiceRequestDto dto)
    {
        var request = await _unitOfWork.ServiceRequestRepository.GetByIdAsync(id);

        if (request == null)
            return Result<AcceptResponseDto>.Fail("Service Request not found");

        if (request.ServiceStatus != ServiceRequestStatus.Pending)
            return Result<AcceptResponseDto>.Fail("Only pending requests can be accepted");

        if (dto.EstimatedCost <= 0)
            return Result<AcceptResponseDto>.Fail("Invalid estimated cost");

        if (dto.EstimatedCompletion <= DateTime.UtcNow)
            return Result<AcceptResponseDto>.Fail("Invalid estimated completion date");

        request.ServiceStatus = ServiceRequestStatus.Accepted;
        request.AcceptedAt = DateTime.UtcNow;
        request.EstimatedCost = dto.EstimatedCost;
        request.EstimatedCompletion = dto.EstimatedCompletion;
        request.TechnicianId = dto.TechnicianId;

        _unitOfWork.ServiceRequestRepository.Update(request);
        await _unitOfWork.CompleteAsync();

        var response = new AcceptResponseDto
        {
            Id = request.Id,
            Status = request.ServiceStatus,
            AcceptedAt = request.AcceptedAt.Value
        };

        return Result<AcceptResponseDto>.Ok(response);
    }

    public async Task<Result<RejectResponseDto>> RejectAsync(int id, RejectServiceRequestDto dto)
    {
        var request = await _unitOfWork.ServiceRequestRepository.GetByIdAsync(id);

        if (request == null)
            return Result<RejectResponseDto>.Fail("Service Request not found");

        if (request.ServiceStatus != ServiceRequestStatus.Pending)
            return Result<RejectResponseDto>.Fail("Only pending requests can be rejected");

        if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            return Result<RejectResponseDto>.Fail("Rejection reason is required");

        request.ServiceStatus = ServiceRequestStatus.Rejected;
        request.RejectedAt = DateTime.UtcNow;
        request.RejectionReason = dto.RejectionReason;

        _unitOfWork.ServiceRequestRepository.Update(request);
        await _unitOfWork.CompleteAsync();

        var response = new RejectResponseDto
        {
            Id = request.Id,
            Status = request.ServiceStatus,
            RejectedAt = request.RejectedAt.Value,
        };

        return Result<RejectResponseDto>.Ok(response);
    }

    public async Task<Result<CompleteResponseDto>> CompleteAsync(int id, CompleteServiceRequestDto dto)
    {
        var request = await _unitOfWork.ServiceRequestRepository.GetByIdAsync(id);

        if (request == null)
            return Result<CompleteResponseDto>.Fail("Service Request not found");

        if (request.ServiceStatus != ServiceRequestStatus.Accepted)
            return Result<CompleteResponseDto>.Fail("Only accepted requests can be completed");

        if (dto.TotalPrice <= 0)
            return Result<CompleteResponseDto>.Fail("Invalid total price");

        request.ServiceStatus = ServiceRequestStatus.Completed;
        request.CompletedAt = DateTime.UtcNow;
        request.TotalPrice = dto.TotalPrice;
        request.TechnicianNotes = dto.TechnicianNotes;

        _unitOfWork.ServiceRequestRepository.Update(request);
        await _unitOfWork.CompleteAsync();

        var response = new CompleteResponseDto
        {
            Id = request.Id,
            Status = request.ServiceStatus,
            CompletedAt = request.CompletedAt.Value
        };

        return Result<CompleteResponseDto>.Ok(response);
    }
    #endregion

    #region History
    public async Task<Result<List<HistoryCardDto>>> GetUserHistoryAsync(string userId)
    {
        var data = await _unitOfWork.ServiceRequestRepository
            .GetUserHistory(userId)
            .Select(x => new HistoryCardDto
            {
                Id = x.Id,
                CarName = x.Vehicle.Brand.Name + " " + x.Vehicle.Model.Name,
                ProviderName = x.ServiceCenter.Name,
                Date = x.CreatedAt,
                Status = x.ServiceStatus,
                PaymentMethod = x.PaymentMethod.ToString(),
                ServiceName = x.ServiceRequestServices
                    .Select(s => s.MaintenanceType.Name)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Result<List<HistoryCardDto>>.Ok(data);
    }

    public async Task<Result<HistoryDetailsDto>> GetUserHistoryDetailsAsync(int id, string userId)
    {
        var history = await _unitOfWork.ServiceRequestRepository
            .GetUserHistory(userId)
            .Where(x => x.Id == id)
            .Select(x => new HistoryDetailsDto
            {
                Id = x.Id,
                CarName = x.Vehicle.Brand + " " + x.Vehicle.Model,
                ProviderName = x.ServiceCenter.Name,
                Date = x.CreatedAt,
                PaymentMethod = x.PaymentMethod.ToString(),

                ServiceName = x.ServiceRequestServices
                    .Select(s => s.MaintenanceType.Name)
                    .FirstOrDefault(),

                ServiceDetails = x.ServiceRequestServices
                    .Select(s => s.Description)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (history == null)
            return Result<HistoryDetailsDto>.Fail("History not found");

        return Result<HistoryDetailsDto>.Ok(history);
    }
    #endregion
}