using CarWare.Application.Common;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services.ServiceRequests
{
    public class ServiceRequestQueryService : IServiceRequestQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ServiceRequestQueryService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        private int CenterId =>
            _currentUserService.ServiceCenterId
            ?? throw new Exception("ServiceCenterId not found");

        public async Task<Result<ServiceRequestListResponse>> GetRequestsAsync(ServiceRequestQueryParams queryParams)
        {
            var query = _unitOfWork.ServiceRequestRepository
                .GetByCenterId(CenterId)
                .OrderByDescending(x => x.CreatedAt);

            var groupedCounts = await query
                .GroupBy(x => x.ServiceStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var counts = new CountsDto
            {
                Pending = groupedCounts
                    .FirstOrDefault(x =>
                        x.Status == ServiceRequestStatus.Pending)?.Count ?? 0,

                Active = groupedCounts
                    .FirstOrDefault(x =>
                        x.Status == ServiceRequestStatus.Accepted)?.Count ?? 0,

                Completed = groupedCounts
                    .FirstOrDefault(x =>
                        x.Status == ServiceRequestStatus.Completed)?.Count ?? 0
            };

            if (!string.IsNullOrWhiteSpace(queryParams.Status)
                && queryParams.Status != "all")
            {
                if (Enum.TryParse<ServiceRequestStatus>(
                    queryParams.Status,
                    true,
                    out var statusEnum))
                {
                    query = (IOrderedQueryable<ServiceRequest>)query
                        .Where(x => x.ServiceStatus == statusEnum);
                }
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var s = queryParams.Search.ToLower();

                query = (IOrderedQueryable<ServiceRequest>)query
                    .Where(x =>
                        EF.Functions.Like(x.User.FullName, $"%{s}%")
                        || EF.Functions.Like(x.Vehicle.Brand.Name, $"%{s}%"));
            }

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
                        Id = x.User.Id,
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
                .GetByIdAsync(id, CenterId);

            if (entity == null)
                return Result<ServiceRequestDto>
                    .Fail("Service Request not found");

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
                    Id = entity.User.Id,
                    Name = entity.User.FullName
                }
            };

            return Result<ServiceRequestDto>.Ok(result);
        }
    }
}