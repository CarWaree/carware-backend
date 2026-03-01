using CarWare.Application.Common;
using CarWare.Application.DTOs.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IHistoryService
    {
        Task<Result<Pagination<HistoryCardDto>>> GetAllAsync(
     string userId,
     string? status,
     int pageNumber,
     int pageSize);

        Task<Result<HistoryDetailsDto>> GetByIdAsync(int id);
    }
}
