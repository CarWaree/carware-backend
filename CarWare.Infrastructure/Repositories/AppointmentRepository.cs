using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AppointmentRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Appointment>> GetUserAppointmentsAsync(string userId)
        {
            return await _dbContext.Appointments
                .Where(u => u.UserId == userId)
                .Include(v => v.Vehicle)
                .Include(sc => sc.ServiceCenter)
                .Include(s => s.Service)
                .OrderBy(d => d.Date)
                .ThenBy(t => t.TimeSlot)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbContext.Appointments
                .Include(a => a.Vehicle)
                .Include(a => a.Service)
                .Include(a => a.ServiceCenter)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

    }
}
