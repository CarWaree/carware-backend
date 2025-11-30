using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using CarWare.Infrastructure.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private Hashtable _repositories;

        // Expose VehicleRepository
        private IVehicleRepository _vehicleRepository;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Hashtable();
        }

        public IVehicleRepository VehicleRepository
            => _vehicleRepository ??= new VehicleRepository(_dbContext);

        public async Task<int> CompleteAsync()
            => await _dbContext.SaveChangesAsync();

        public async ValueTask DisposeAsync()
            => await _dbContext.DisposeAsync();

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var key = typeof(TEntity).Name;
            if (!_repositories.ContainsKey(key))
            {
                var repository = new GenericRepository<TEntity>(_dbContext);
                _repositories.Add(key, repository);
            }
            return _repositories[key] as IGenericRepository<TEntity>;
        }
    }
}
