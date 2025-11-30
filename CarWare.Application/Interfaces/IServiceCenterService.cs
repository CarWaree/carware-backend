using CarWare.Application.Common;
using CarWare.Application.DTOs.Provider_Center;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IServiceCenterService
    {
        Task<Result<IEnumerable<ServiceCenterDto>>> GetAllAsync();
        Task<Result<IEnumerable<ServiceCenterDto>>> GetByServiceTypeAsync(int serviceTypeId);

    }
}
