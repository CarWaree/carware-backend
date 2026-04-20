using CarWare.Application.Common;
using CarWare.Application.DTOs.History;
using CarWare.Application.DTOs.ServiceRequests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IServiceRequestService
    {
        // Dashboard
        Task<Result<ServiceRequestListResponse>> GetDashboardRequestsAsync(ServiceRequestQueryParams queryParams);
        Task<Result<ServiceRequestDto>> GetRequestDetailsAsync(int id);

        // Workflow
        Task<Result<AcceptResponseDto>> AcceptAsync(int id, AcceptServiceRequestDto dto);
        Task<Result<RejectResponseDto>> RejectAsync(int id, RejectServiceRequestDto dto);
        Task<Result<CompleteResponseDto>> CompleteAsync(int id, CompleteServiceRequestDto dto);

        // History (User)
        Task<Result<List<HistoryCardDto>>> GetUserHistoryAsync(string userId);

        Task<Result<HistoryDetailsDto>> GetUserHistoryDetailsAsync(int requestId, string userId);
    }
}