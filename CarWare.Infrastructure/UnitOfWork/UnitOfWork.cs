using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using CarWare.Infrastructure.Repositories;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private Hashtable _repositories;

        // Expose Repositories
        private IVehicleRepository _vehicleRepository;
        private IMaintenanceRepository _maintenanceReminderRepository;
        private IAppointmentRepository _appointmentRepository;
        private IServiceRequestRepository _serviceRequestRepository;
        private INotificationRepository _notificationRepository;
        private IDeviceTokenRepository _deviceTokenRepository;
        private IServiceCenterRepository _serviceCenterRepository;
        private IMaintenanceTypeRepository _maintenanceTypeRepository;
        private IProviderServicesRepository _providerServicesRepository;
        private ISlotRepository _slotRepository;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Hashtable();
        }

        public IVehicleRepository VehicleRepository
            => _vehicleRepository ??= new VehicleRepository(_dbContext);

        public IMaintenanceRepository MaintenanceRepository
            => _maintenanceReminderRepository ??= new MaintenanceRepository(_dbContext);

        public IAppointmentRepository AppointmentRepository
            => _appointmentRepository ??= new AppointmentRepository(_dbContext);

        public IServiceRequestRepository ServiceRequestRepository
            => _serviceRequestRepository ??= new ServiceRequestRepository(_dbContext);

        public INotificationRepository NotificationRepository
            => _notificationRepository ??= new NotificationRepository(_dbContext);
        public IDeviceTokenRepository DeviceTokenRepository
            => _deviceTokenRepository ??= new DeviceTokenRepository(_dbContext);

        public IServiceCenterRepository ServiceCenterRepository
            => _serviceCenterRepository ??= new ServiceCenterRepository(_dbContext);

        public IMaintenanceTypeRepository MaintenanceTypeRepository
            => _maintenanceTypeRepository ??= new MaintenanceTypeRepository(_dbContext);

        public IProviderServicesRepository ProviderServicesRepository
            => _providerServicesRepository ??= new ProviderServicesRepository(_dbContext);

        public ISlotRepository SlotRepository
            => _slotRepository ??= new SlotRepository(_dbContext);

        public async Task<int> CompleteAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

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
