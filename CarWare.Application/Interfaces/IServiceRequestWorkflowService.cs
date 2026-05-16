using CarWare.Application.Common;
using CarWare.Application.DTOs.ServiceRequests;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IServiceRequestWorkflowService
    {
        Task<Result<AcceptResponseDto>>
            AcceptAsync(int id, AcceptServiceRequestDto dto);

        Task<Result<RejectResponseDto>>
            RejectAsync(int id, RejectServiceRequestDto dto);

        Task<Result<CompleteResponseDto>>
            CompleteAsync(int id, CompleteServiceRequestDto dto);
    }
}