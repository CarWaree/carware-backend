using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Repositories
{
    public class ServiceRequestRepository : GenericRepository<ServiceRequest>, IServiceRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // Base Query -> Full Include
        public IQueryable<ServiceRequest> GetAllQueryable()
        {
            return _context.ServiceRequest
                .Include(x => x.User)
                .Include(x => x.Vehicle)
                .Include(x => x.ServiceCenter)
                .Include(x => x.Appointment)
                .Include(x => x.ServiceRequestServices)
                    .ThenInclude(s => s.MaintenanceType);
        }

        // Filtered by Center [List/Dashboard]
        public IQueryable<ServiceRequest> GetByCenterId(int centerId)
        {
            return _context.ServiceRequest
                .Where(x => x.ServiceCenterId == centerId)
                .Include(x => x.User)
                .Include(x => x.Vehicle)
                .Include(x => x.ServiceCenter)
                .Include(x => x.Appointment)
                .Include(x => x.ServiceRequestServices)
                    .ThenInclude(s => s.MaintenanceType);
        }

        // Single Record (by Details)
        public async Task<ServiceRequest> GetByIdAsync(int id, int centerId)
        {
            return await _context.ServiceRequest
                .Include(x => x.User)
                .Include(x => x.Vehicle)
                .Include(x => x.ServiceCenter)
                .Include(x => x.Appointment)
                .Include(x => x.ServiceRequestServices)
                .ThenInclude(x => x.MaintenanceType)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ServiceCenterId == centerId);
        }

        // Dashboard stat: today's requests count
        public async Task<int> CountTodayByCenterIdAsync(int centerId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.ServiceRequest
                .Where(x => x.ServiceCenterId == centerId &&
                            x.CreatedAt.Date == today)
                .CountAsync();
        }

        // Dashboard stat: today's income (completed requests only) 
        public async Task<decimal> SumTodayIncomeByCenterIdAsync(int centerId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.ServiceRequest
                .Where(x => x.ServiceCenterId == centerId &&
                            x.ServiceStatus == ServiceRequestStatus.Completed &&
                            x.CompletedAt.HasValue &&
                            x.CompletedAt.Value.Date == today)
                .SumAsync(x => x.TotalPrice);
        }

        // Dashboard calendar: appointments within week 
        public IQueryable<ServiceRequest> GetWeeklyAppointments
            (int centerId, DateTime weekStart, DateTime weekEnd)
        {
            return _context.ServiceRequest
                .Where(x => x.ServiceCenterId == centerId &&
                            x.Appointment != null &&
                            x.Appointment.Date >= weekStart &&
                            x.Appointment.Date < weekEnd)
                .Include(x => x.Appointment)
                .Include(x => x.Vehicle).ThenInclude(v => v.Brand)
                .Include(x => x.Vehicle).ThenInclude(v => v.Model)
                .Include(x => x.ServiceRequestServices)
                    .ThenInclude(s => s.MaintenanceType);
        }

        // History [Completed || Rejected] By User
        public IQueryable<ServiceRequest> GetUserHistory(string userId)
        {
            return _context.ServiceRequest
                .Where(x => x.UserId == userId &&
                       (x.ServiceStatus == ServiceRequestStatus.Completed ||
                        x.ServiceStatus == ServiceRequestStatus.Rejected))
                .Include(x => x.Vehicle)
                .Include(x => x.ServiceCenter)
                .Include(x => x.ServiceRequestServices)
                    .ThenInclude(s => s.MaintenanceType);
        }
    }
}