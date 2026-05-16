using CarWare.Application.Common;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IServiceRequestQueryService
    {
        Task<Result<ServiceRequestListResponse>>
            GetRequestsAsync(ServiceRequestQueryParams queryParams);

        Task<Result<ServiceRequestDto>>
            GetRequestDetailsAsync(int id);
    }
}