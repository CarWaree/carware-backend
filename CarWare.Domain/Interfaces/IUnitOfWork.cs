using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Domain
{
    public interface IUnitOfWork:IAsyncDisposable
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        IVehicleRepository VehicleRepository { get; }
        Task<int> CompleteAsync();
    }
}
