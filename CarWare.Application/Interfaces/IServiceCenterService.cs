using CarWare.Application.Common;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.DTOs.Slots;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IServiceCenterService
    {
        Task<Result<List<MaintenanceTypeDto>>> GetCenterServicesAsync(int centerId);
        Task<Result<CenterSlotsResponseDto>> GetCenterSlotsAsync(int centerId);
    }
}