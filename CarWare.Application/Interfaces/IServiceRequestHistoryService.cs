using CarWare.Application.Common;
using CarWare.Application.DTOs.History;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IServiceRequestHistoryService
    {
        Task<Result<List<HistoryCardDto>>>
            GetUserHistoryAsync(string userId);

        Task<Result<HistoryDetailsDto>>
            GetUserHistoryDetailsAsync(int requestId, string userId);
    }
}