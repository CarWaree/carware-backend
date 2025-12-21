using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace CarWare.Domain
{
    public interface IUnitOfWork:IAsyncDisposable
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        IVehicleRepository VehicleRepository { get; }
        IMaintenanceRepository MaintenanceRepository { get; }
        IAppointmentRepository AppointmentRepository { get; }
        Task<int> CompleteAsync();
    }
}