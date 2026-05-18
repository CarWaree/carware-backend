using CarWare.Application.Common;
using CarWare.Application.DTOs.Dashboard;
using CarWare.Application.DTOs.Dashboard.Response;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<Result<DashboardResponse>> GetDashboardAsync(DashboardQueryParams queryParams);
    }
}