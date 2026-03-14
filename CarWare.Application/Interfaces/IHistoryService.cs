using CarWare.Application.Common;
using CarWare.Application.DTOs.History;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IHistoryService
    {
        Task<Result<List<HistoryCardDto>>> GetAllAsync(string userId);

        Task<Result<HistoryDetailsDto>> GetByIdAsync(int id, string userId);
    }
}