using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
        public DbSet<MaintenanceType> maintenanceTypes { get; set; }
        public DbSet<ServiceCenter> ServiceCenters { get; set; }
        public DbSet<ProviderServices> ProviderServices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            builder.Entity<Vehicle>()
            .HasOne(c => c.user)           
            .WithMany(c => c.vehicles)                     
            .HasForeignKey(c => c.UserId)  
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Model>()
                .HasOne(m => m.Brand)
                .WithMany(b => b.Models)
                .HasForeignKey(m => m.BrandId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Brand)
                .WithMany(b => b.Vehicles)
                .HasForeignKey(v => v.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Model)
                .WithMany(m => m.Vehicles)
                .HasForeignKey(v => v.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaintenanceReminder>()
                .HasOne(m => m.Vehicle)
                .WithMany(v => v.maintenances)
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MaintenanceReminder>()
                .HasOne(m => m.Type)
                .WithMany(t => t.Reminders)
                .HasForeignKey(m => m.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

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

            builder.Entity<ProviderServices>()
                .HasKey(ps => new { ps.ServiceCenterId, ps.ServiceId });

            builder.Entity<ProviderServices>()
                .HasOne(ps => ps.ServiceCenter)
                .WithMany(sc => sc.ProviderServices)
                .HasForeignKey(ps => ps.ServiceCenterId);

            builder.Entity<ProviderServices>()
                .HasOne(ps => ps.Service)
                .WithMany(s => s.ProviderServices)
                .HasForeignKey(ps => ps.ServiceId);

            builder.Entity<ServiceCenter>().HasData(
                new ServiceCenter { Id = 1, Name = "Provider 1", Location = "Nasr City", Phone = "0123456789" },
                new ServiceCenter { Id = 2, Name = "Provider 2", Location = "Maadi", Phone = "0112233445" },
                 new ServiceCenter { Id = 3, Name = "Provider 3", Location = "Heliopolis", Phone = "01011223344" },
                new ServiceCenter { Id = 4, Name = "Provider 4", Location = "6th October", Phone = "01233445566" },
                new ServiceCenter { Id = 5, Name = "Provider 5", Location = "Dokki", Phone = "01099887766" }
            );

            builder.Entity<Appointment>()
                .HasOne(a => a.user)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId);

            builder.Entity<Appointment>()
                .HasOne(a => a.Vehicle)
                .WithMany(v => v.Appointments)
                .HasForeignKey(a => a.VehicleId);

            builder.Entity<Appointment>()
                .HasOne(a => a.ServiceCenter)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceCenterId);

            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId);
        }
    }
}