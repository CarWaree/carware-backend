using CarWare.Application.Common;
using CarWare.Application.DTOs.Provider_Center;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IServiceCenterService
    {
        Task<Result<IEnumerable<ServiceCenterDto>>> GetAllAsync();
        Task<Result<IEnumerable<ServiceCenterDto>>> GetByServiceTypeAsync(int serviceTypeId);

    }
}
