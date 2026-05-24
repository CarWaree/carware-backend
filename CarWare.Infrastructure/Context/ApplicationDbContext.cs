using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarWare.Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public ApplicationDbContext() { }

        //DbSets
        public DbSet<Vehicle> vehicles { get; set; }
        public DbSet<Brand> brands { get; set; }
        public DbSet<Model> models { get; set; }
        public DbSet<MaintenanceReminder> maintenances { get; set; }
        public DbSet<MaintenanceType> MaintenanceTypes { get; set; }
        public DbSet<ServiceCenter> ServiceCenters { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<ProviderServices> ProviderServices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ServiceRequest> ServiceRequest { get; set; }
        public DbSet<ServiceRequestItem> ServiceRequestItem { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            //Service
            builder.Entity<MaintenanceType>().HasData(

                new MaintenanceType { Id = 1, Name = "Oil Change" },
                new MaintenanceType { Id = 2, Name = "Brake Fluid" },
                new MaintenanceType { Id = 3, Name = "Tires & Battery Services" },
                new MaintenanceType { Id = 4, Name = "Engine Check" },
                new MaintenanceType { Id = 5, Name = "General Service" },
                new MaintenanceType { Id = 6, Name = "Transmission Service" },
                new MaintenanceType { Id = 7, Name = "Electric Services" },
                new MaintenanceType { Id = 8, Name = "Body & Paint Services" },
                new MaintenanceType { Id = 9, Name = "Suspension Services" }
            );

            //Center
            builder.Entity<ServiceCenter>().HasData(
                new ServiceCenter { Id = 1, Name = "Provider 1", Location = "Nasr City", Phone = "0123456789" },
                new ServiceCenter { Id = 2, Name = "Provider 2", Location = "Maadi", Phone = "0112233445" },
                 new ServiceCenter { Id = 3, Name = "Provider 3", Location = "Heliopolis", Phone = "01011223344" },
                new ServiceCenter { Id = 4, Name = "Provider 4", Location = "6th October", Phone = "01233445566" },
                new ServiceCenter { Id = 5, Name = "Provider 5", Location = "Dokki", Phone = "01099887766" }
            );
        }
    }
}