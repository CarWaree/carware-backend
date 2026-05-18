using CarWare.Application.Common;
using CarWare.Application.DTOs.History;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services.ServiceRequests
{
    public class ServiceRequestHistoryService
        : IServiceRequestHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRequestHistoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<HistoryCardDto>>>
            GetUserHistoryAsync(string userId)
        {
            var data = await _unitOfWork.ServiceRequestRepository
                .GetUserHistory(userId)
                .Select(x => new HistoryCardDto
                {
                    Id = x.Id,

                    CarName =
                        x.Vehicle.Brand.Name + " " +
                        x.Vehicle.Model.Name,

                    ProviderName = x.ServiceCenter.Name,

                    Date = x.CreatedAt,

                    Status = x.ServiceStatus,

                    PaymentMethod = x.PaymentMethod.ToString(),

                    ServiceName = x.ServiceRequestServices
                        .Select(s => s.MaintenanceType.Name)
                        .FirstOrDefault(),

                    TotalPrice = x.TotalPrice
                })
                .ToListAsync();

            return Result<List<HistoryCardDto>>.Ok(data);
        }

        public async Task<Result<HistoryDetailsDto>>
            GetUserHistoryDetailsAsync(int requestId, string userId)
        {
            var history = await _unitOfWork.ServiceRequestRepository
                .GetUserHistory(userId)
                .Where(x => x.Id == requestId)
                .Select(x => new HistoryDetailsDto
                {
                    Id = x.Id,

                    CarName =
                        x.Vehicle.Brand.Name + " " +
                        x.Vehicle.Model.Name,

                    ProviderName = x.ServiceCenter.Name,

                    Date = x.CreatedAt,

                    PaymentMethod = x.PaymentMethod.ToString(),

                    ServiceName = x.ServiceRequestServices
                        .Select(s => s.MaintenanceType.Name)
                        .FirstOrDefault(),

                    ServiceDetails = x.TechnicianNotes,

                    TotalPrice = x.TotalPrice,
                })
                .FirstOrDefaultAsync();

            if (history == null)
                return Result<HistoryDetailsDto>
                    .Fail("History not found");

            return Result<HistoryDetailsDto>.Ok(history);
        }
    }
}