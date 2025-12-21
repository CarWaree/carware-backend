using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Domain.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<List<Appointment>> GetUserAppointmentsAsync(string userId);
    }
}