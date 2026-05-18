using CarWare.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IServiceRequestRepository : IGenericRepository<ServiceRequest>
    {
        // ── Service Reqeust Screen ──
        IQueryable<ServiceRequest> GetAllQueryable();
        Task<ServiceRequest> GetByIdAsync(int id, int centerId);
        IQueryable<ServiceRequest> GetByCenterId(int centerId);
        Task<int> CountTodayByCenterIdAsync(int centerId);
        Task<decimal> SumTodayIncomeByCenterIdAsync(int centerId);
        IQueryable<ServiceRequest> GetWeeklyAppointments
            (int centerId, DateTime weekStart, DateTime weekEnd);
        IQueryable<ServiceRequest> GetUserHistory(string userId);

        //IQueryable<ServiceRequest> GetCenterHistory(string userId);
    }
}